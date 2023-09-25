using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Functions.Timelines.Helpers;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.QueueTriggers
{
    public class PatchTimelinesByDeleteTweetTrigger
    {
        private const string FUNCTION_NAME = "PatchTimelinesByDeleteTweetTrigger";
        private readonly CosmosClient _client;

        public PatchTimelinesByDeleteTweetTrigger(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public Task PatchTimelinesByDeleteTweetAsync(
            [QueueTrigger(AZURE_STORAGE_PATCH_TIMELINES_BY_DELETE_TWEET_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
            ILogger logger) => _client.PatchTimelineAsync<PatchTimelinesByDeleteTweet>(myQueueItem, FUNCTION_NAME, logger);
    }
}
