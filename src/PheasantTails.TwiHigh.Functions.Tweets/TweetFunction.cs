using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Data.Model;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets
{
    public class TweetFunction
    {
        private readonly ILogger<TweetFunction> _logger;
        private readonly CosmosClient _client;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TweetFunction(CosmosClient client, ILogger<TweetFunction> log, TokenValidationParameters tokenValidationParameters)
        {
            _logger = log;
            _client = client;
            _tokenValidationParameters = tokenValidationParameters;
        }
        

        

        [FunctionName("UpdateTweetByUpdatedUserInfoTrigger")]
        public async Task UpdateTweetByUpdatedUserInfoTriggerAsync([QueueTrigger(AZURE_STORAGE_UPDATE_USER_INFO_IN_TWEET_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null)
                {
                    return;
                }

                var user = JsonSerializer.Deserialize<UpdateUserQueue>(myQueueItem).TwiHighUser;
                var patch = new[]
                {
                    PatchOperation.Set("/userDisplayId", user.DisplayId),
                    PatchOperation.Set("/userDisplayName", user.DisplayName),
                    PatchOperation.Set("/userAvatarUrl", user.AvatarUrl),
                    // あえてDateTimeOffset.UtcNowにしてる。タイムラインの更新時刻が同時刻であると。タイムライン取得時に50件を超過する。
                    PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow)
                };

                var query = new QueryDefinition("SELECT VALUE c.id FROM c ORDER BY c.createAt DESC");
                var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var iterator = tweets.GetItemQueryIterator<Guid>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(user.Id.ToString())
                });
                var batchTasks = new List<Task<ItemResponse<Tweet>>>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    foreach (var tweetId in response)
                    {
                        patch[3] = PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow);
                        var task = tweets.PatchItemAsync<Tweet>(
                            tweetId.ToString(),
                            new PartitionKey(user.Id.ToString()),
                            patch);
                        batchTasks.Add(task);
                    }
                }
                var batchResult = await Task.WhenAll(batchTasks);
                foreach (var result in batchResult)
                {
                    if ((int)result.StatusCode < 200 || 300 <= (int)result.StatusCode)
                    {
                        continue;
                    }

                    await QueueStorages.InsertMessageAsync(
                        AZURE_STORAGE_UPDATE_USER_INFO_IN_TIMELINE_QUEUE_NAME,
                        new UpdateTimelineQueue(result));
                }

                _logger.LogInformation("Batch finish. RU:{0}, Count:{1}, Success:{2}",
                    batchResult.Sum(r => r.Headers.RequestCharge),
                    batchResult.Length,
                    batchResult.LongCount(r => 200 <= (int)r.StatusCode && (int)r.StatusCode < 300));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex, ex.StackTrace);
                throw;
            }
        }
    }
}

