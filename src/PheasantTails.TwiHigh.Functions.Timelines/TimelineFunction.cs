using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Followers;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Extensions;
using System;
using System.Collections.Generic;
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
            if (!tweets.Any())
            {
                return new NoContentResult();
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

        [FunctionName("GetMyTimelineV2")]
        public async Task<IActionResult> GetMyTimelineV2Async(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "timeline")] HttpRequest req)
        {
            if (!req.TryGetUserId(out var id))
            {
                return new UnauthorizedResult();
            }

            // 検索範囲を設定
            var sinceDatetime = DateTimeOffset.MinValue;
            var untilDatetime = DateTimeOffset.MaxValue;
            if (req.Query.TryGetValue("since", out var since) && DateTimeOffset.TryParse(since, out var tmpSinceDatetime))
            {
                sinceDatetime = tmpSinceDatetime;
            }
            if (req.Query.TryGetValue("until", out var until) && DateTimeOffset.TryParse(until, out var tmpUntilDatetime))
            {
                untilDatetime = tmpUntilDatetime;
            }

            // クエリの作成
            var query = new QueryDefinition(
                "SELECT TOP 50 * FROM c " +
                "WHERE c.ownerUserId = @OwnerUserId " +
                "AND @SinceDatetime < c.updateAt " +
                "AND c.updateAt <= @UntilDatetime " +
                "ORDER BY c.createAt DESC")
                .WithParameter("@OwnerUserId", id)
                .WithParameter("@SinceDatetime", sinceDatetime)
                .WithParameter("@UntilDatetime", untilDatetime);

            var timelines = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);
            var iterator = timelines.GetItemQueryIterator<Timeline>(query,
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
            if (!tweets.Any())
            {
                return new NoContentResult();
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

        [FunctionName("DeleteTimelinesTweetTrigger")]
        public async Task DeleteTimelinesTweetTriggerAsync([QueueTrigger(AZURE_STORAGE_DELETE_TIMELINES_TWEET_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null) return;

                var que = JsonSerializer.Deserialize<DeleteTimelineQueue>(myQueueItem);
                var tasks = new List<Task>();
                var patch = new[]
                {
                    PatchOperation.Set("/text", "This tweet has been deleted."),
                    PatchOperation.Set("/isDeleted", true),
                    PatchOperation.Set("/updateAt", que.Tweet.UpdateAt)
                };
                var timelines = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);

                // クエリの作成
                var query = new QueryDefinition(
                    "SELECT c.id, c.ownerUserId FROM c " +
                    "WHERE c.tweetId = @TweetId")
                    .WithParameter("@TweetId", que.Tweet.Id);

                var iterator = timelines.GetItemQueryIterator<TimelineIdOwnerUserIdPair>(query);

                while (iterator.HasMoreResults)
                {
                    var result = await iterator.ReadNextAsync();
                    foreach (var pair in result.Resource)
                    {
                        tasks.Add(timelines
                            .PatchItemAsync<Timeline>(
                                id: pair.Id.ToString(),
                                partitionKey: new PartitionKey(pair.OwnerUserId.ToString()),
                                patchOperations: patch,
                                requestOptions: new PatchItemRequestOptions { IfMatchEtag = result.ETag })
                            );
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (CosmosException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private class TimelineIdOwnerUserIdPair
        {
            public Guid Id { get; set; }
            public Guid OwnerUserId { get; set; }
        }
    }
}
