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

        [FunctionName("GetMyTimeline")]
        public async Task<IActionResult> GetMyTimelineAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = null)] HttpRequest req)
        {
            if (!req.TryGetUserId(_tokenValidationParameters, out var id))
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
            if (!req.TryGetUserId(_tokenValidationParameters, out var id))
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
                "SELECT TOP 100 * FROM c " +
                "WHERE (@SinceDatetime < c.updateAt AND c.updateAt <= @UntilDatetime) " +
                "OR (@SinceDatetime < c.createAt AND c.createAt <= @UntilDatetime) " +
                "ORDER BY c.createAt DESC")
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
            var que = JsonSerializer.Deserialize<AddNewFolloweeTweetContext>(myQueueItem);
            _logger.LogInformation("[AddTimelinesFollowTrigger] {0} follow to {1} at {2}.", que.UserId, que.FolloweeId, DateTimeOffset.UtcNow.ToString("yyyy/MM/dd HH:mm:ss"));

            // 自身のタイムラインにの対象ユーザの既存のツイートがある場合は削除する
            var query = new QueryDefinition(
                "SELECT c.id, c.ownerUserId FROM c " +
                "WHERE c.userId = @FolloweeId " +
                "AND c.ownerUserId = @OwnerUserId")
                .WithParameter("@FolloweeId", que.FolloweeId)
                .WithParameter("@OwnerUserId", que.UserId);
            _logger.LogInformation("[AddTimelinesFollowTrigger] Remove Query: {0}", query.QueryText);

            var timelines = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);
            var batch = timelines.CreateTransactionalBatch(new PartitionKey(que.UserId.ToString()));
            var timelineIterator = timelines.GetItemQueryIterator<TimelineIdOwnerUserIdPair>(query);
            while (timelineIterator.HasMoreResults)
            {
                var result = await timelineIterator.ReadNextAsync();
                foreach (var pair in result.Resource)
                {
                    batch.DeleteItem(pair.Id.ToString());
                }
            }

            // 対象ユーザのツイートを取得
            var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
            var tweetIterator = tweets.GetItemQueryIterator<Tweet>(
                "SELECT * FROM c " +
                "WHERE c.isDeleted != true",
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(que.FolloweeId.ToString())
                });


            // 自身のタイムラインに加える
            var index = 0;
            while (tweetIterator.HasMoreResults)
            {
                var result = await tweetIterator.ReadNextAsync();
                foreach (var tweet in result.Resource)
                {
                    var timeline = new Timeline(que.UserId, tweet)
                    {
                        UpdateAt = DateTimeOffset.UtcNow
                    };
                    batch.CreateItem(timeline);
                    index++;
                    if (100 <= index)
                    {
                        var response = await batch.ExecuteAsync();
                        _logger.LogInformation("[AddTimelinesFollowTrigger] Batch status code:{0}, RU:{1}", response.StatusCode, response.RequestCharge);
                        batch = timelines.CreateTransactionalBatch(new PartitionKey(que.UserId.ToString()));
                        index = 0;
                    }
                }
            }

            var response2 = await batch.ExecuteAsync();
            _logger.LogInformation("[AddTimelinesFollowTrigger] Batch status code:{0}, RU:{1}", response2.StatusCode, response2.RequestCharge);
        }

        [FunctionName("DeleteTimelinesTweetTrigger")]
        public async Task DeleteTimelinesTweetTriggerAsync([QueueTrigger(AZURE_STORAGE_DELETE_TIMELINES_TWEET_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null) return;
                var que = JsonSerializer.Deserialize<DeleteTimelineQueue>(myQueueItem);
                var patch = new[]
                {
                    PatchOperation.Set("/text", "This tweet has been deleted."),
                    PatchOperation.Set("/isDeleted", true),
                    PatchOperation.Set("/updateAt", que.Tweet.UpdateAt)
                };
                await PatchTimelineAsync(que.Tweet.Id, patch);
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

        [FunctionName("UpdateReplyFromTimelinesTweetTrigger")]
        public async Task UpdateReplyFromTimelinesTweetTriggerAsync([QueueTrigger(AZURE_STORAGE_UPDATE_REPLYFROM_TIMELINES_TWEET_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null) return;

                var que = JsonSerializer.Deserialize<UpdateTimelineQueue>(myQueueItem);

                if (!que.Tweet.ReplyTo.HasValue)
                {
                    throw new ArgumentException("ReplyTo property is required.", nameof(myQueueItem));
                }

                var patch = new[]
                {
                    PatchOperation.Add("/replyFrom/-", que.Tweet.Id),
                    PatchOperation.Set("/updateAt", que.Tweet.UpdateAt)
                };
                await PatchTimelineAsync(que.Tweet.ReplyTo.Value, patch);
            }
            catch (CosmosException ex)
            {
                throw;
            }
        }

        [FunctionName("UpdateReplyToTimelinesTweetTrigger")]
        public async Task UpdateReplyToTimelinesTweetTriggerAsync([QueueTrigger(AZURE_STORAGE_UPDATE_REPLYTO_TIMELINES_TWEET_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null) return;
                var que = JsonSerializer.Deserialize<UpdateTimelineQueue>(myQueueItem);
                var patch = new[]
                {
                    PatchOperation.Remove("/replyTo"),
                    PatchOperation.Set("/updateAt", que.Tweet.UpdateAt)
                };
                await PatchTimelineAsync(que.Tweet.Id, patch);
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

        [FunctionName("UpdateUserInfoTriggeredTweetUpdated")]
        public async Task UpdateUserInfoTriggeredTweetUpdatedAsync([QueueTrigger(AZURE_STORAGE_UPDATE_USER_INFO_IN_TIMELINE_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null) return;
                var que = JsonSerializer.Deserialize<UpdateTimelineQueue>(myQueueItem);
                var patch = new[]
                {
                    PatchOperation.Set("/userDisplayId", que.Tweet.UserDisplayId),
                    PatchOperation.Set("/userDisplayName", que.Tweet.UserDisplayName),
                    PatchOperation.Set("/userAvatarUrl", que.Tweet.UserAvatarUrl),
                    PatchOperation.Set("/updateAt", que.Tweet.UpdateAt)
                };
                await PatchTimelineAsync(que.Tweet.Id, patch);
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

        private async Task PatchTimelineAsync(Guid tweetId, IReadOnlyList<PatchOperation> patch)
        {
            var timelines = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);

            // クエリの作成
            var query = new QueryDefinition(
                "SELECT c.id, c.ownerUserId FROM c " +
                "WHERE c.tweetId = @TweetId")
                .WithParameter("@TweetId", tweetId);

            var iterator = timelines.GetItemQueryIterator<TimelineIdOwnerUserIdPair>(query);

            var tasks = new List<Task<ResponseMessage>>();
            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();
                foreach (var pair in result)
                {
                    tasks.Add(timelines
                        .PatchItemStreamAsync(
                            id: pair.Id.ToString(),
                            partitionKey: new PartitionKey(pair.OwnerUserId.ToString()),
                            patchOperations: patch,
                            requestOptions: new PatchItemRequestOptions { IfMatchEtag = result.ETag })
                        );
                }
            }
            var batchResult = await Task.WhenAll(tasks);
            _logger.LogInformation("PatchTimelineAsync batch finish. RU:{0}, Task Count:{1}, Success:{2}",
                batchResult.Sum(r => r.Headers.RequestCharge),
                batchResult.LongLength,
                batchResult.LongCount(r => r.IsSuccessStatusCode));
        }

        [FunctionName("DeleteTimelinesFollowTrigger")]
        public async Task DeleteTimelinesFollowTriggerAsync([QueueTrigger(AZURE_STORAGE_DELETE_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            if (myQueueItem == null)
            {
                return;
            }

            // キューの取得
            var que = JsonSerializer.Deserialize<RemoveFolloweeTweetContext>(myQueueItem);
            var now = DateTimeOffset.UtcNow;

            _logger.LogInformation("{0} remove to {1} at {2}.", que.UserId, que.FolloweeId, now.ToString("yyyy/MM/dd HH:mm:ss"));
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
                    _logger.LogInformation("id: {0}, ownerUserId:{1}", pair.Id, pair.OwnerUserId);
                }
            }
            var response = await batch.ExecuteAsync();
            _logger.LogInformation("Batch status code:{0}, RU:{1}", response.StatusCode, response.RequestCharge);
        }

        private class TimelineIdOwnerUserIdPair
        {
            public Guid Id { get; set; }
            public Guid OwnerUserId { get; set; }
        }
    }
}
