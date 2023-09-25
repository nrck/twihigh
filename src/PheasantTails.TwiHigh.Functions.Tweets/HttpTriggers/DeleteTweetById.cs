using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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
using System.Net;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets.HttpTriggers
{
    public class DeleteTweetById
    {
        private const string FUNCTION_NAME = "DelateTweetById";
        private readonly CosmosClient _client;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public DeleteTweetById(CosmosClient client, TokenValidationParameters tokenValidationParameters)
        {
            _client = client;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task<IActionResult> DeleteTweetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tweets/{id}")] HttpRequest req,
            string id,
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

                // Create patch operations.
                var patch = new[]
                {
                    // Sets the deleted flag to true.
                    PatchOperation.Set("/isDeleted", true),
                    // Sets the update time to UTC now.
                    PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow)
                };

                // Delete the tweet.
                ItemResponse<Tweet> tweetPatchResponse;
                try
                {
                    // If the UserID of the posting user of the target tweet and the UserID of
                    // the requesting user are different, an exception will occur here.
                    tweetPatchResponse = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME)
                        .PatchItemAsync<Tweet>(id, new PartitionKey(userId), patch);
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.BadRequest)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, "The delete request is bad request.");
                        return new BadRequestResult();
                    }
                    if (ex.StatusCode == HttpStatusCode.Conflict)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, "The delete request is conflict.");
                        return new ConflictResult();
                    }
                    throw new TweetException($"An error occurred while deleting the tweet.", ex);
                }

                // Remove from a follower's timeline.
                var que = new PatchTweetQueue { TweetId = tweetPatchResponse.Resource.Id, };
                que.AppendOperation(PatchOperationType.Set, "/text", "This tweet has been deleted.")
                    .AppendOperation(PatchOperationType.Set, "/isDeleted", true)
                    .AppendOperation(PatchOperationType.Set, "/updateAt", tweetPatchResponse.Resource.UpdateAt);
                await QueueStorages.InsertMessageAsync(
                    AZURE_STORAGE_PATCH_TWEET_IN_TIMELINES_QUEUE_NAME,
                    que);

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
