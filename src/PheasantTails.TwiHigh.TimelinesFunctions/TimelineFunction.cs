using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.DataStore.Entity;
using PheasantTails.TwiHigh.Extensions;
using PheasantTails.TwiHigh.Model.Followers;
using PheasantTails.TwiHigh.Model.Timelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.FunctionCore.StaticStrings;

namespace PheasantTails.TwiHigh.TimelinesFunctions
{
    public class TimelineFunction
    {
        private readonly ILogger<TimelineFunction> _logger;
        private readonly CosmosClient _client;

        public TimelineFunction(CosmosClient client, ILogger<TimelineFunction> log)
        {
            _logger = log;
            _client = client;
        }

        [FunctionName("GetMyTimeline")]
        public async Task<IActionResult> GetMyTimelineAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = null)] HttpRequest req)
        {
            if (!req.TryGetUserId(out var id))
            {
                return new UnauthorizedResult();
            }

            var timelines = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);
            var iterator = timelines.GetItemQueryIterator<Timeline>(
                "SELECT * FROM c ORDER BY c.createAt DESC",
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(id)
                });

            var tweets = new List<Tweet>();
            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();
                foreach (var timeline in result.Resource)
                {
                    tweets.Add(timeline.ToTweet());
                }
            }

            var latest = tweets.Max(t => t.CreateAt);
            var oldest = tweets.Min(t => t.CreateAt);
            var response = new ResponseTimelineContext
            {
                Latest = latest,
                Oldest = oldest,
                Tweets = tweets.OrderByDescending(t => t.CreateAt).ToArray()
            };

            return new OkObjectResult(response);
        }

        [FunctionName("AddTimelinesTweetTrigger")]
        public async Task AddTimelinesTweetTriggerAsync([QueueTrigger(AZURE_STORAGE_ADD_TIMELINES_TWEET_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
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

        [FunctionName("AddTimelinesFollowTrigger")]
        public async Task AddTimelinesFollowTriggerAsync([QueueTrigger(AZURE_STORAGE_ADD_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            if (myQueueItem == null)
            {
                return;
            }

            // キューの取得
            var context = JsonSerializer.Deserialize<AddNewFolloweeTweetContext>(myQueueItem);

            // 対象ユーザのツイートを取得
            var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
            var iterator = tweets.GetItemQueryIterator<Tweet>(
                "SELECT * FROM c",
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(context.FolloweeId.ToString())
                });

            // 自身のタイムラインに加える
            var timelines = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);
            var batch = timelines.CreateTransactionalBatch(new PartitionKey(context.UserId.ToString()));
            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();
                foreach (var tweet in result.Resource)
                {
                    var timeline = new Timeline(context.UserId, tweet);
                    batch.CreateItem(timeline);
                }
                await batch.ExecuteAsync();
            }
        }
    }
}
