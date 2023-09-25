using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Functions.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets.HttpTriggers
{
    public class PutFavorite
    {
        private const string FUNCTION_NAME = "PutFavorite";
        private readonly CosmosClient _client;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public PutFavorite(CosmosClient client, TokenValidationParameters tokenValidationParameters)
        {
            _client = client;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task<IActionResult> PutFavoriteTweetAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "tweets/{tweetId}/favorite")] HttpRequest req,
            Guid tweetId,
            ILogger logger)
        {
            try
            {
                logger.TwiHighLogStart(FUNCTION_NAME);

                // Check authroized.
                if (!req.TryGetUserId(_tokenValidationParameters, out var userId))
                {
                    logger.TwiHighLogWarning(FUNCTION_NAME, "Cannot get a user id from JWT.");
                    return new UnauthorizedResult();
                }

                // Get the user from cosmos db.
                ItemResponse<TwiHighUser> userReadResponse;
                try
                {
                    userReadResponse = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME)
                        .ReadItemAsync<TwiHighUser>(userId, new PartitionKey(userId));
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, ex);
                        logger.TwiHighLogWarning(FUNCTION_NAME, "Authroized user is NOT found. ID: {0}", userId);
                        return new UnauthorizedResult();
                    }
                    throw new TweetException($"An error occurred while retrieving the user. UserId: {userId}", ex);
                }
                logger.TwiHighLogInformation(FUNCTION_NAME, "Authroized user is found. ID: {0}", userReadResponse.Resource.Id);
                logger.TwiHighLogInformation(FUNCTION_NAME, "Favorite by {0}", userReadResponse.Resource.DisplayId);

                // Get target tweet.
                var tweetContainer = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
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
                }
                catch (CosmosException ex)
                {
                    throw new TweetException($"An error occurred while getting tweet items by a linq query.", ex);
                }

                // Create favorite context.
                var favoriteFrom = new IdTimeStampPair
                {
                    Id = Guid.Parse(userId),
                    TimeStamp = DateTimeOffset.UtcNow
                };

                // Validates that the tweet is not a favorite of the requesting user.
                if (targetTweet.FavoriteFrom.Any(pair => pair.Id == favoriteFrom.Id))
                {
                    logger.TwiHighLogWarning(FUNCTION_NAME, "This tweet is already favorites by request user. Tweet id: {0}, User id: {1}.", tweetId, userId);
                    return new BadRequestResult();
                }

                // Create patch operations.
                var patch = new[]
                {
                    PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow)
                };
                var queOperation = new[]
                {
                    TweetPatchOperation.Set("/updateAt", DateTimeOffset.UtcNow.ToString())
                };
                if (targetTweet.FavoriteFrom.Length == 0)
                {
                    patch = patch.Append(PatchOperation.Set("/favoriteFrom", new[] { favoriteFrom })).ToArray();
                    queOperation = queOperation.Append(TweetPatchOperation.Set("/favoriteFrom", JsonSerializer.Serialize(new[] { favoriteFrom }))).ToArray();
                }
                else
                {
                    patch = patch.Append(PatchOperation.Add("/favoriteFrom/-", favoriteFrom)).ToArray();
                    queOperation = queOperation.Append(TweetPatchOperation.Add("/favoriteFrom/-", JsonSerializer.Serialize(favoriteFrom))).ToArray();
                }

                // Patch the tweet.
                ItemResponse<Tweet> tweetPatchResponse;
                try
                {
                    // TODO: e-tag
                    tweetPatchResponse = await tweetContainer
                        .PatchItemAsync<Tweet>(targetTweet.Id.ToString(), new PartitionKey(targetTweet.UserId.ToString()), patch);
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.BadRequest)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, ex);
                        logger.TwiHighLogWarning(FUNCTION_NAME, "The put favorite request is bad request.");
                        return new BadRequestResult();
                    }
                    if (ex.StatusCode == HttpStatusCode.Conflict)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, ex);
                        logger.TwiHighLogWarning(FUNCTION_NAME, "The put favorite request is conflict.");
                        return new ConflictResult();
                    }
                    throw new TweetException($"An error occurred while put favorite the tweet.", ex);
                }

                // Insert queue message to timelines function.
                var patchTweetQue = new PatchTweetQueue
                {
                    TweetId = tweetId,
                    Operations = queOperation
                };
                await QueueStorages.InsertMessageAsync(
                    AZURE_STORAGE_PATCH_TWEET_IN_TIMELINES_QUEUE_NAME,
                    patchTweetQue);
                // Insert queue message to feeds function.
                await QueueStorages.InsertMessageAsync(
                    AZURE_STORAGE_FEED_FAVORED_BY_USER_QUEUE_NAME,
                    new FeedFavoredQueue(targetTweet, userReadResponse));

                return new OkResult();
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
    }
}
