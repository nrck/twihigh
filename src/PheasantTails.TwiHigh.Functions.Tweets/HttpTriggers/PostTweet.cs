﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Core.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Functions.Core.Queues;
using System;
using System.Net;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets.HttpTriggers
{
    public class PostTweet
    {
        private const string FUNCTION_NAME = "PostTweet";
        private readonly CosmosClient _client;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public PostTweet(CosmosClient client, TokenValidationParameters tokenValidationParameters)
        {
            _client = client;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task<IActionResult> PostTweetAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tweets/")] HttpRequest req,
            ILogger logger)
        {
            try
            {
                logger.TwiHighLogStart(FUNCTION_NAME);

                // Check authroized.
                if (!req.TryGetUserId(_tokenValidationParameters, out var id))
                {
                    logger.TwiHighLogWarning(FUNCTION_NAME, "Cannot get a user id from JWT.");
                    return new UnauthorizedResult();
                }

                // Get the user from cosmos db.
                ItemResponse<TwiHighUser> userReadResponse;
                try
                {
                    userReadResponse = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME)
                        .ReadItemAsync<TwiHighUser>(id, new PartitionKey(id));
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, "Authroized user is NOT found. ID: {0}", id);
                        return new UnauthorizedResult();
                    }
                    throw new TweetException($"An error occurred while retrieving the user. UserId: {id}", ex);
                }
                logger.TwiHighLogInformation(FUNCTION_NAME, "Authroized user is found. ID: {0}", userReadResponse.Resource.Id);
                logger.TwiHighLogInformation(FUNCTION_NAME, "Post by {0}", userReadResponse.Resource.DisplayId);

                // Deserialize context
                var context = await req.JsonDeserializeAsync<PostTweetContext>();

                // Create new a tweet object.
                var now = DateTimeOffset.UtcNow;
                var tweet = new Tweet
                {
                    Id = Guid.NewGuid(),
                    Text = context.Text,
                    ReplyTo = context.ReplyTo?.TweetId,
                    UserId = userReadResponse.Resource.Id,
                    UserDisplayId = userReadResponse.Resource.DisplayId,
                    UserDisplayName = userReadResponse.Resource.DisplayName,
                    UserAvatarUrl = userReadResponse.Resource.AvatarUrl,
                    IsDeleted = false,
                    UpdateAt = now,
                    CreateAt = now
                };

                // Create tweet item.
                ItemResponse<Tweet> tweetCreateResponse;
                try
                {
                    tweetCreateResponse = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME)
                        .CreateItemAsync(tweet);
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.BadRequest)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, "Posted tweet is bad request.");
                        return new BadRequestObjectResult(context);
                    }
                    if (ex.StatusCode == HttpStatusCode.Conflict)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, "Posted tweet is conflict.");
                        return new ConflictObjectResult(context);
                    }
                    throw new TweetException($"An error occurred while inserting the tweet.", ex);
                }
                logger.TwiHighLogInformation(FUNCTION_NAME, "Posted tweet is created. ID: {0}", tweetCreateResponse.Resource.Id);
                logger.TwiHighLogInformation(FUNCTION_NAME, "{0} > {1}", tweetCreateResponse.Resource.UserDisplayId, tweetCreateResponse.Resource.Text);

                // If posted tweet has ReplyTo property, insert UpdateTimelineQueue.
                if (context.ReplyTo != null)
                {
                    await InsertUpdateTimelineQueueAsync(context.ReplyTo, tweetCreateResponse, logger);
                }

                // Insert queue message to timelines.
                await QueueStorages.InsertMessageAsync(
                    AZURE_STORAGE_ADD_TIMELINES_TWEET_TRIGGER_QUEUE_NAME,
                    new AddTimelineByPostTweetQueue(tweet, userReadResponse.Resource.Followers));
                return new CreatedResult($"/{userReadResponse.Resource.DisplayId}/Status/{tweetCreateResponse.Resource.Id}", tweetCreateResponse.Resource);
            }
            catch (Exception ex)
            {
                logger.TwiHighLogError(FUNCTION_NAME, ex);
                throw;
            }
            finally
            {
                logger.TwiHighLogEnd(FUNCTION_NAME);
            }
        }

        private async Task<ItemResponse<Tweet>> InsertUpdateTimelineQueueAsync(ReplyToContext context, Tweet tweet, ILogger logger)
        {
            var funcName = nameof(InsertUpdateTimelineQueueAsync);
            try
            {
                logger.TwiHighLogStart(funcName);

                // Create a patch operation context.
                var tweetid = context.TweetId.ToString();
                var partitionKey = new PartitionKey(context.UserId.ToString());
                var patch = new[]
                {
                    PatchOperation.Add("/replyFrom/-", tweet.Id),
                    PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow)
                };

                // Do patch operation.
                ItemResponse<Tweet> tweetPatchResponse = null;
                try
                {
                    tweetPatchResponse = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME)
                        .PatchItemAsync<Tweet>(tweetid, partitionKey, patch);
                    logger.TwiHighLogInformation(funcName, "Patch a tweet. Total RU: {0:0.00}", tweetPatchResponse.RequestCharge);

                    var que = new PatchTimelinesByAddReplyFromQueue
                    {
                        TweetId = context.TweetId,
                        AddReplyFrom = tweet.Id
                    };

                    // update user timelines
                    await QueueStorages.InsertMessageAsync(
                        AZURE_STORAGE_PATCH_TIMELINES_BY_ADD_REPLYFROM_QUEUE_NAME,
                        que);
                    // insert feed
                    var feedQueue = new FeedMentionedQueue
                    {
                        FeedByUserId = tweet.UserId,
                        FeedByUserPartitionKey = tweet.UserId.ToString(),
                        TargetTweetId = context.TweetId,
                        TargetTweetPartitionKey = context.UserId.ToString(),
                        FeedByTweetId = tweet.Id,
                        FeedByTweetPartitionKey = tweet.GetPartitionKeyString()
                    };
                    await QueueStorages.InsertMessageAsync(
                        AZURE_STORAGE_FEED_MENTIONED_BY_USER_QUEUE_NAME,
                        feedQueue);

                    logger.TwiHighLogInformation(funcName, "Insert queue message to {0}", AZURE_STORAGE_FEED_MENTIONED_BY_USER_QUEUE_NAME);
                }
                catch (CosmosException ex)
                {
                    logger.TwiHighLogError(FUNCTION_NAME, ex);

                    // Delete the replyTo of own tweet when it fails.
                    var que = new PatchTimelinesByRemoveReplyToQueue
                    {
                        TweetId = context.TweetId
                    };
                    await QueueStorages.InsertMessageAsync(
                        AZURE_STORAGE_PATCH_TIMELINES_BY_REMOVE_REPLYTO_QUEUE_NAME,
                        que);
                    logger.TwiHighLogInformation(funcName, "Insert queue message to {0}", AZURE_STORAGE_PATCH_TIMELINES_BY_REMOVE_REPLYTO_QUEUE_NAME);
                }

                return tweetPatchResponse;
            }
            catch (Exception ex)
            {
                logger.TwiHighLogError(funcName, ex);
                throw;
            }
            finally
            {
                logger.TwiHighLogEnd(funcName);
            }
        }
    }
}
