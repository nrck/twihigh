using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Functions.Timelines.Helpers;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.QueueTriggers
{
    public class PatchTimelinesByAddFavoriteFromTrigger
    {
        private const string FUNCTION_NAME = "PatchTimelinesByAddFavoriteFromTrigger";
        private readonly CosmosClient _client;

        public PatchTimelinesByAddFavoriteFromTrigger(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public Task PatchTimelinesByAddFavoriteFromAsync(
            [QueueTrigger(AZURE_STORAGE_PATCH_TIMELINES_BY_ADD_FAVORITE_FROM_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
            ILogger logger) => _client.PatchTimelineAsync<PatchTimelinesByAddFavoriteFromQueue>(myQueueItem, FUNCTION_NAME, logger);
    }
}
