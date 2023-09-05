﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Functions.Timelines.Helpers;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.QueueTriggers
{
    public class UpdateUserInfoTriggeredTweetUpdated
    {

        private const string FUNCTION_NAME = "UpdateUserInfoTriggeredTweetUpdated";
        private readonly CosmosClient _client;

        public UpdateUserInfoTriggeredTweetUpdated(CosmosClient client)
        {
            _client = client;
        }
        [FunctionName(FUNCTION_NAME)]
        public async Task UpdateUserInfoTriggeredTweetUpdatedAsync(
            [QueueTrigger(AZURE_STORAGE_UPDATE_USER_INFO_IN_TIMELINE_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
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
                var que = JsonSerializer.Deserialize<UpdateTimelineQueue>(myQueueItem);
                var patch = new[]
                {
                    PatchOperation.Set("/userDisplayId", que.Tweet.UserDisplayId),
                    PatchOperation.Set("/userDisplayName", que.Tweet.UserDisplayName),
                    PatchOperation.Set("/userAvatarUrl", que.Tweet.UserAvatarUrl),
                    PatchOperation.Set("/updateAt", que.Tweet.UpdateAt)
                };
                var batchResult = await _client.PatchTimelineAsync(que.Tweet.Id, patch);
                logger.TwiHighLogInformation(FUNCTION_NAME, "PatchTimelineAsync batch finish. RU:{0}, Task Count:{1}, Success:{2}",
                    batchResult.Sum(r => r.Headers.RequestCharge),
                    batchResult.LongLength,
                    batchResult.LongCount(r => r.IsSuccessStatusCode));
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
