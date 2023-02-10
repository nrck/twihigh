using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.DataStore.Entity;
using PheasantTails.TwiHigh.Model.Timelines;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.FunctionCore.StaticStrings;

namespace PheasantTails.TwiHigh.TimelinesFunctions
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly CosmosClient _client;

        public Function1(CosmosClient client, ILogger<Function1> log)
        {
            _logger = log;
            _client = client;
        }

        [FunctionName("AddTimeline")]
        public async Task AddTimelineAsync([QueueTrigger(AZURE_STORAGE_ADD_TIMELINE_QUEUE_NAME, Connection = "ConnectionString")] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null) return;

                var que = JsonSerializer.Deserialize<QueAddTimelineContext>(myQueueItem);
                var tasks = new List<Task>();
                await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME).CreateItemAsync(new Timeline(que.Tweet.UserId, que.Tweet));

                foreach (var user in que.Followers)
                {
                    tasks.Add(_client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME).CreateItemAsync(new Timeline(user, que.Tweet)));
                }

                await Task.WhenAll(tasks);
            }
            catch (CosmosException ex)
            {

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
