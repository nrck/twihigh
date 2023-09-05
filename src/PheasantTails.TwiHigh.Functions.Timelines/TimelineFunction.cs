using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Data.Model.Followers;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Extensions;
using PheasantTails.TwiHigh.Functions.Timelines.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines
{
    public class TimelineFunction
    {
        private readonly ILogger<TimelineFunction> _logger;
        private readonly CosmosClient _client;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TimelineFunction(CosmosClient client, ILogger<TimelineFunction> log, TokenValidationParameters tokenValidationParameters)
        {
            _logger = log;
            _client = client;
            _tokenValidationParameters = tokenValidationParameters;
        }






        


        


        [FunctionName("DeleteTimelinesFollowTrigger")]
        public async Task DeleteTimelinesFollowTriggerAsync([QueueTrigger(AZURE_STORAGE_DELETE_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            if (myQueueItem == null)
            {
                return;
            }

            // �L���[�̎擾
            var que = JsonSerializer.Deserialize<RemoveFolloweeTweetContext>(myQueueItem);
            var now = DateTimeOffset.UtcNow;

            _logger.LogInformation("{0} remove to {1} at {2}.", que.UserId, que.FolloweeId, now.ToString("yyyy/MM/dd HH:mm:ss"));
            var patch = new PatchOperation[]
            {
                PatchOperation.Set("/text", "This tweet has been deleted."),
                PatchOperation.Set("/isDeleted", true),
                PatchOperation.Set("/updateAt", now)
            };

            // �t�H���[���O�����l�̃c�C�[�g���擾����
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
                    _logger.LogInformation("id: {0}, ownerUserId:{1}", pair.Id, pair.OwnerUserId);
                }
            }
            var response = await batch.ExecuteAsync();
            _logger.LogInformation("Batch status code:{0}, RU:{1}", response.StatusCode, response.RequestCharge);
        }
    }
}
