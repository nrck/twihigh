using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Functions.Core.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Functions.Core.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.QueueTriggers
{
    public class AddTimelinesFollowTrigger
    {
        private const string FUNCTION_NAME = "AddTimelinesFollowTrigger";
        private const string QUERY_PARM_FOLLOWEE_ID = "@FolloweeId";
        private const string QUERY_PARM_OWNER_ID = "@OwnerUserId";
        private readonly CosmosClient _client;
        private static readonly QueryDefinition _queryDefinition = new($"""
            SELECT c.id, c.ownerUserId FROM c 
            WHERE c.userId = {QUERY_PARM_FOLLOWEE_ID} 
            AND c.ownerUserId = {QUERY_PARM_OWNER_ID}
            """);

        public AddTimelinesFollowTrigger(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task AddTimelinesFollowTriggerAsync(
            [QueueTrigger(AZURE_STORAGE_ADD_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
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

                var que = JsonSerializer.Deserialize<AddTimelinesByNewFolloweeQueue>(myQueueItem);

                // Create query
                var query = _queryDefinition
                    .WithParameter(QUERY_PARM_FOLLOWEE_ID, que.FolloweeId)
                    .WithParameter(QUERY_PARM_OWNER_ID, que.UserId);

                var timelineContainer = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);
                var timelineIterator = timelineContainer.GetItemQueryIterator<TimelineIdOwnerUserIdPair>(query);
                var tasks = new List<Task<ItemResponse<Timeline>>>();
                while (timelineIterator.HasMoreResults)
                {
                    var result = await timelineIterator.ReadNextAsync();
                    var task = result.Select(
                        pair => timelineContainer.DeleteItemAsync<Timeline>(
                            pair.Id.ToString(),
                            new PartitionKey(pair.OwnerUserId.ToString())))
                        .ToArray();
                    tasks.AddRange(task);
                }

                // Execute
                var batchResult = await Task.WhenAll(tasks);

                // 対象ユーザのツイートを取得
                var tweetContainer = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var tweetIterator = tweetContainer.GetItemQueryIterator<Tweet>(
                    "SELECT * FROM c " +
                    "WHERE c.isDeleted != true OR NOT IS_DEFINED(c.isDeleted)",
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(que.FolloweeId.ToString())
                    });


                // 自身のタイムラインに加える
                var index = 0;
                var batch = timelineContainer.CreateTransactionalBatch(new PartitionKey(que.UserId.ToString()));
                while (tweetIterator.HasMoreResults)
                {
                    var result = await tweetIterator.ReadNextAsync();
                    foreach (var tweet in result.Resource)
                    {
                        var timeline = new Timeline(que.UserId, tweet)
                        {
                            UpdateAt = DateTimeOffset.UtcNow
                        };
                        batch.CreateItem(timeline);
                        index++;
                        if (100 <= index)
                        {
                            var response = await batch.ExecuteAsync();
                            logger.TwiHighLogInformation(FUNCTION_NAME, "Batch status code:{0}, RU:{1}", response.StatusCode, response.RequestCharge);
                            batch = timelineContainer.CreateTransactionalBatch(new PartitionKey(que.UserId.ToString()));
                            index = 0;
                        }
                    }
                }

                var response2 = await batch.ExecuteAsync();
                logger.TwiHighLogInformation(FUNCTION_NAME, "Batch status code:{0}, RU:{1}", response2.StatusCode, response2.RequestCharge);
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
