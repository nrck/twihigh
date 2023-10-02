using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Functions.Core.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets.QueueTriggers
{
    public class UpdateTweetByUpdatedUserInfoTrigger
    {
        private const string FUNCTION_NAME = "UpdateTweetByUpdatedUserInfoTrigger";
        private readonly CosmosClient _client;
        private static readonly QueryDefinition _queryDefinition = new($"""
            SELECT VALUE c.id FROM c ORDER BY c.createAt DESC
            """);

        public UpdateTweetByUpdatedUserInfoTrigger(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task UpdateTweetByUpdatedUserInfoTriggerAsync(
            [QueueTrigger(AZURE_STORAGE_UPDATE_USER_INFO_IN_TWEET_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
            ILogger logger)
        {
            try
            {
                logger.TwiHighLogStart(FUNCTION_NAME);
                if (myQueueItem == null)
                {
                    // null check
                    throw new ArgumentNullException(nameof(myQueueItem), "Queue is Null");
                }

                // Create patch operation.
                var user = JsonSerializer.Deserialize<UpdateUserQueue>(myQueueItem).TwiHighUser;
                var patch = new[]
                {
                    PatchOperation.Set("/userDisplayId", user.DisplayId),
                    PatchOperation.Set("/userDisplayName", user.DisplayName),
                    PatchOperation.Set("/userAvatarUrl", user.AvatarUrl),
                    PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow)
                };

                // Get id of user tweets.
                var tweetContainer = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var iterator = tweetContainer.GetItemQueryIterator<Guid>(_queryDefinition, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(user.Id.ToString())
                });

                // Create patch item tasks.
                var batchTasks = new List<Task<ItemResponse<Tweet>>>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    foreach (var tweetId in response)
                    {
                        patch[3] = PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow);
                        var task = tweetContainer.PatchItemAsync<Tweet>(
                            tweetId.ToString(),
                            new PartitionKey(user.Id.ToString()),
                            patch);
                        batchTasks.Add(task);
                    }
                }

                // Exequte tasks.
                logger.TwiHighLogInformation(FUNCTION_NAME, "{0} patch operations start.", batchTasks.Count);
                var batchResult = await Task.WhenAll(batchTasks);
                logger.TwiHighLogInformation(FUNCTION_NAME, "{0} patch operations finish.", batchTasks.Count);

                // Check patch result.
                foreach (var result in batchResult)
                {
                    if ((int)result.StatusCode < 200 || 300 <= (int)result.StatusCode)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, "{0} response status code: {1}", result.Headers.Location, (int)result.StatusCode);
                        continue;
                    }

                    var que = new PatchTimelinesByUpdateUserInfoQueue
                    {
                        TweetId = result.Resource.Id,
                        SetUserDisplayId = result.Resource.UserDisplayId,
                        SetUserDisplayName = result.Resource.UserDisplayName,
                        SetUserAvatarUrl = result.Resource.UserAvatarUrl
                    };
                    await QueueStorages.InsertMessageAsync(
                        AZURE_STORAGE_PATCH_TIMELINES_BY_UPDATE_USER_INFO_NAME,
                        que);
                }

                logger.TwiHighLogWarning(FUNCTION_NAME, "Queue trigger finish. RU:{0}, Count:{1}, Success:{2}",
                    batchResult.Sum(r => r.Headers.RequestCharge),
                    batchResult.Length,
                    batchResult.LongCount(r => 200 <= (int)r.StatusCode && (int)r.StatusCode < 300));
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
