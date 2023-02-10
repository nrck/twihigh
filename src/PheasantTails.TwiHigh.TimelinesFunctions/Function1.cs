using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.DataStore.Entity;
using PheasantTails.TwiHigh.Model.Timelines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        public async Task AddTimelineAsync([QueueTrigger("add-timelines", Connection = "ConnectionString")] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null) return;

                var que = JsonSerializer.Deserialize<QueAddTimelineContext>(myQueueItem);
                var tasks = new List<Task>();
                await _client.GetContainer("TwiHighDB", "Timelines").CreateItemAsync(new Timeline(que.Tweet.UserId, que.Tweet));

                foreach (var user in que.Followers)
                {
                    tasks.Add(_client.GetContainer("TwiHighDB", "Timelines").CreateItemAsync(new Timeline(user, que.Tweet)));
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
