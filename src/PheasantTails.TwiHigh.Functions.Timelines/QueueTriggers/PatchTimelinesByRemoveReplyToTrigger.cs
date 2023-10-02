using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Functions.Core.Queues;
using PheasantTails.TwiHigh.Functions.Timelines.Helpers;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.QueueTriggers
{
    public class PatchTimelinesByRemoveReplyToTrigger
    {
        private const string FUNCTION_NAME = "PatchTimelinesByRemoveReplyToTrigger";
        private readonly CosmosClient _client;

        public PatchTimelinesByRemoveReplyToTrigger(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public Task PatchTimelinesByRemoveReplyToAsync(
            [QueueTrigger(AZURE_STORAGE_PATCH_TIMELINES_BY_REMOVE_REPLYTO_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
            ILogger logger) => _client.PatchTimelineAsync<PatchTimelinesByRemoveReplyToQueue>(myQueueItem, FUNCTION_NAME, logger);
    }
}
