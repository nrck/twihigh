using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.QueueTriggers
{
    public class AddTimelinesTweetTrigger
    {
        private const string FUNCTION_NAME = "AddTimelinesTweetTrigger";
        private readonly CosmosClient _client;

        public AddTimelinesTweetTrigger(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task AddTimelinesTweetTriggerAsync(
            [QueueTrigger(AZURE_STORAGE_ADD_TIMELINES_TWEET_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
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

                var que = JsonSerializer.Deserialize<QueAddTimelineContext>(myQueueItem);

                // Add the tweet to your own timeline.
                var timelineContainer = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);
                var requestCharge = 0.0;
                try
                {
                    var response = await timelineContainer.CreateItemAsync(new Timeline(que.Tweet.UserId, que.Tweet));
                    requestCharge = response.RequestCharge;
                }
                catch (CosmosException ex)
                {
                    throw new TimelineException($"An error occurred while creating timeline item to own timeline. TweetId: {que.Tweet.Id}", ex);
                }

                // Add the tweet to followers timeline.
                try
                {
                    var tasks = new List<Task<ItemResponse<Timeline>>>();
                    foreach (var userId in que.Followers)
                    {
                        var task = timelineContainer.CreateItemAsync(new Timeline(userId, que.Tweet));
                        tasks.Add(task);
                    }

                    // Exequte
                    var batchResult = await Task.WhenAll(tasks);
                    logger.TwiHighLogWarning(FUNCTION_NAME, "Queue trigger finish. RU:{0}, Count:{1}, Success:{2}",
                        batchResult.Sum(r => r.RequestCharge),
                        batchResult.Length,
                        batchResult.LongCount(r => 200 <= (int)r.StatusCode && (int)r.StatusCode < 300));
                }
                catch (CosmosException ex)
                {
                    throw new TimelineException($"An error occurred while creating timeline item to followers timeline. TweetId: {que.Tweet.Id}", ex);
                }
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
