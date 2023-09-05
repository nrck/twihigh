using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Followers;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.QueueTriggers
{
    public  class DeleteTimelinesFollowTrigger
    {
        private const string FUNCTION_NAME = "DeleteTimelinesFollowTrigger";
        private readonly CosmosClient _client;

        public DeleteTimelinesFollowTrigger(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task DeleteTimelinesFollowTriggerAsync(
            [QueueTrigger(AZURE_STORAGE_DELETE_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
            ILogger logger)
        {
            if (myQueueItem == null)
            {
                return;
            }

            // キューの取得
            var que = JsonSerializer.Deserialize<RemoveFolloweeTweetContext>(myQueueItem);
            var now = DateTimeOffset.UtcNow;

            logger.LogInformation("{0} remove to {1} at {2}.", que.UserId, que.FolloweeId, now.ToString("yyyy/MM/dd HH:mm:ss"));
            var patch = new PatchOperation[]
            {
                PatchOperation.Set("/text", "This tweet has been deleted."),
                PatchOperation.Set("/isDeleted", true),
                PatchOperation.Set("/updateAt", now)
            };

            // フォローを外した人のツイートを取得する
            var query = new QueryDefinition(
                "SELECT c.id, c.ownerUserId FROM c " +
                "WHERE c.userId = @FolloweeId " +
                "AND c.ownerUserId = @OwnerUserId")
                .WithParameter("@FolloweeId", que.FolloweeId)
                .WithParameter("@OwnerUserId", que.UserId);

            var timelines = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);
            var batch = timelines.CreateTransactionalBatch(new PartitionKey(que.UserId.ToString()));
            var iterator = timelines.GetItemQueryIterator<TimelineIdOwnerUserIdPair>(query);
            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();
                foreach (var pair in result.Resource)
                {
                    batch.PatchItem(
                        id: pair.Id.ToString(),
                        patchOperations: patch,
                        requestOptions: new TransactionalBatchPatchItemRequestOptions { IfMatchEtag = result.ETag }
                    );
                    logger.LogInformation("id: {0}, ownerUserId:{1}", pair.Id, pair.OwnerUserId);
                }
            }
            var response = await batch.ExecuteAsync();
            logger.LogInformation("Batch status code:{0}, RU:{1}", response.StatusCode, response.RequestCharge);
        }
    }
}
