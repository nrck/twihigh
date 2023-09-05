using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets.HttpTriggers
{
    public class GetTweetById
    {
        private const string FUNCTION_NAME = "GetTweetById";
        private const int THREAD_MAX_LENGTH = 5;
        private const int REPLY_FROM_MAX_LENGTH = 5;
        private readonly CosmosClient _client;

        public GetTweetById(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task<IActionResult> GetTweetByIdV2Async(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{tweetId}")] HttpRequest req,
            Guid tweetId,
            ILogger logger)
        {
            try
            {
                logger.TwiHighLogStart(FUNCTION_NAME);

                var thread = new List<Tweet>();

                var tweetContainer = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);

                // Get target tweet.
                Tweet targetTweet;
                try
                {
                    targetTweet = (await tweetContainer.GetItemLinqQueryable<Tweet>()
                        .Where(t => t.Id == tweetId && t.IsDeleted != true)
                        .ToFeedIterator()
                        .ReadNextAsync())
                        .FirstOrDefault();
                    if (targetTweet == null)
                    {
                        logger.TwiHighLogInformation(FUNCTION_NAME, "{0} is Not Found.", tweetId);
                        return new NotFoundResult();
                    }
                    logger.TwiHighLogInformation(FUNCTION_NAME, "Target tweet is founded. ID: {0}", targetTweet.Id);
                    logger.TwiHighLogInformation(FUNCTION_NAME, "{0} > {1}", targetTweet.UserDisplayId, targetTweet.Text);
                    thread.Add(targetTweet);
                }
                catch (CosmosException ex)
                {
                    throw new TweetException($"An error occurred while getting tweet items by a linq query.", ex);
                }

                // Get the reply ID of the tweet from the target tweet.
                if (0 < targetTweet.ReplyFrom.Length)
                {
                    logger.TwiHighLogInformation(FUNCTION_NAME, "The target tweet has reply from {0} tweets.", targetTweet.ReplyFrom.Length);
                    await GetReplyFromTweets(thread, tweetContainer, targetTweet);
                }
                else
                {
                    logger.TwiHighLogInformation(FUNCTION_NAME, "The target tweet has no reply from another tweet.");
                }

                // Get the reply destination retroactively.
                if (targetTweet.ReplyTo.HasValue)
                {
                    logger.TwiHighLogInformation(FUNCTION_NAME, "The target tweet has reply.");
                    await GetReplayToTweets(logger, thread, tweetContainer, targetTweet);
                }
                else
                {
                    logger.TwiHighLogInformation(FUNCTION_NAME, "The target tweet has no reply.");
                }

                // Sort tweet order by created at time.
                if (thread.Any())
                {
                    thread = thread.OrderBy(t => t.CreateAt).ToList();
                    return new OkObjectResult(thread);
                }
                else
                {
                    throw new TweetException("An error. This tweets thread is not found.");
                }
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

        private static async Task GetReplayToTweets(ILogger logger, List<Tweet> thread, Container tweetContainer, Tweet targetTweet)
        {
            try
            {
                Guid? id = targetTweet.ReplyTo;
                while (id != null && thread.Count <= THREAD_MAX_LENGTH)
                {
                    var tweet = (await tweetContainer.GetItemLinqQueryable<Tweet>()
                        .Where(t => t.Id == id && t.IsDeleted != true)
                        .ToFeedIterator()
                        .ReadNextAsync())
                        .FirstOrDefault();
                    if (tweet == null)
                    {
                        logger.TwiHighLogInformation(FUNCTION_NAME, "{0} is Not Found.", id);
                        break;
                    }
                    logger.TwiHighLogInformation(FUNCTION_NAME, "Reply to tweet is founded. ID: {0}", tweet.Id);
                    logger.TwiHighLogInformation(FUNCTION_NAME, "{0} > {1}", tweet.UserDisplayId, tweet.Text);

                    // Add tweet to thread.
                    thread.Add(tweet);

                    // Sets next id.
                    id = tweet.ReplyTo;
                }
            }
            catch (CosmosException ex)
            {
                throw new TweetException($"An error occurred while getting tweet items by a linq query.", ex);
            }
        }

        private static async Task GetReplyFromTweets(List<Tweet> thread, Container tweetContainer, Tweet targetTweet)
        {
            Guid[] replayFromIds;
            if (targetTweet.ReplyFrom.Length < REPLY_FROM_MAX_LENGTH)
            {
                replayFromIds = targetTweet.ReplyFrom;
            }
            else
            {
                replayFromIds = targetTweet.ReplyFrom.Skip(targetTweet.ReplyFrom.Length - REPLY_FROM_MAX_LENGTH).ToArray();
            }

            try
            {
                var iterator = tweetContainer.GetItemLinqQueryable<Tweet>()
                    .Where(t => replayFromIds.Contains(t.Id) && t.IsDeleted != true)
                    .ToFeedIterator();
                while (iterator.HasMoreResults)
                {
                    var tweets = await iterator.ReadNextAsync();
                    thread.AddRange(tweets);
                }
            }
            catch (CosmosException ex)
            {
                throw new TweetException($"An error occurred while getting tweet items by a linq query.", ex);
            }
        }
    }
}
