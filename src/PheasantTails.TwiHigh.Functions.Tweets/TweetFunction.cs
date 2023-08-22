using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Data.Model;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets
{
    public class TweetFunction
    {
        private readonly ILogger<TweetFunction> _logger;
        private readonly CosmosClient _client;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TweetFunction(CosmosClient client, ILogger<TweetFunction> log, TokenValidationParameters tokenValidationParameters)
        {
            _logger = log;
            _client = client;
            _tokenValidationParameters = tokenValidationParameters;
        }

        

        [FunctionName("DelateTweetById")]
        public async Task<IActionResult> DeleteTweetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tweets/{id}")] HttpRequest req,
            string id)
        {
            try
            {
                if (!req.TryGetUserId(_tokenValidationParameters, out var userId))
                {
                    _logger.LogWarning("ユーザIDを取得できませんでした。");
                    return new UnauthorizedResult();
                }

                // 削除対象のツイートに削除フラグを立てる
                var patch = new[]
                {
                    // 削除フラグ
                    PatchOperation.Set("/isDeleted", true),
                    // 更新日時を現在時刻に
                    PatchOperation.Set("/updateAt", DateTimeOffset.Now)
                };

                // 自分のツイート以外のIDが指定されても、ここで例外が発生するはず
                try
                {
                    var tweet = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME)
                        .PatchItemAsync<Tweet>(id, new PartitionKey(userId), patch);

                    // フォロワーのタイムラインから削除する
                    await QueueStorages.InsertMessageAsync(
                        AZURE_STORAGE_DELETE_TIMELINES_TWEET_TRIGGER_QUEUE_NAME,
                        new DeleteTimelineQueue(tweet));
                }
                catch (CosmosException ex)
                {
                    return new StatusCodeResult((int)ex.StatusCode);
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("GetTweetByIdV1")]
        public async Task<IActionResult> GetTweetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tweetsV1/{tweetId}")] HttpRequest req,
            Guid tweetId)
        {
            try
            {
                var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var query = new QueryDefinition(
                    "SELECT * FROM c " +
                    "WHERE (c.replyTo = @TweetId OR c.id = @TweetId OR ARRAY_CONTAINS(c.replyFrom, @TweetId)) " +
                    "AND c.isDeleted != true " +
                    "ORDER BY c.creatAt")
                    .WithParameter("@TweetId", tweetId);

                var iterator = tweets.GetItemQueryIterator<Tweet>(query);

                var tweet = new List<Tweet>();
                while (iterator.HasMoreResults)
                {
                    var res = await iterator.ReadNextAsync();
                    tweet.AddRange(res);
                }
                if (tweet.Count > 0)
                {
                    return new OkObjectResult(tweet);
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [FunctionName("GetTweetById")]
        public async Task<IActionResult> GetTweetByIdV2Async(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tweets/{tweetId}")] HttpRequest req,
            Guid tweetId)
        {
            try
            {
                // 遡りツイートを最大5件取る
                const int THREAD_MAX_LENGTH = 5;
                // リプライツイートを最大5件取る
                const int REPLY_FROM_MAX_LENGTH = 5;
                var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var thread = new List<Tweet>();

                // 対象のツイートを取得する
                var iterator = tweets.GetItemLinqQueryable<Tweet>().Where(t => t.Id == tweetId && t.IsDeleted != true).ToFeedIterator();
                var result = await iterator.ReadNextAsync();
                var tweet = result.FirstOrDefault();
                if (tweet == null)
                {
                    // ツイートが存在しないときは404でリターン
                    return new NotFoundResult();
                }
                thread.Add(tweet);

                // 対象のツイートへのリプライを取得する。
                var replayFromIds = tweet.ReplyFrom.Length < REPLY_FROM_MAX_LENGTH ?
                    tweet.ReplyFrom :
                    tweet.ReplyFrom.Skip(tweet.ReplyFrom.Length - REPLY_FROM_MAX_LENGTH).ToArray();
                iterator = tweets.GetItemLinqQueryable<Tweet>()
                    .Where(t => replayFromIds.Contains(t.Id) && t.IsDeleted != true)
                    .ToFeedIterator();
                while (iterator.HasMoreResults)
                {
                    thread.AddRange(await iterator.ReadNextAsync());
                }

                // リプライ先を遡って取得する
                Guid? id = tweet.ReplyTo;
                while (id != null && thread.Count <= THREAD_MAX_LENGTH)
                {
                    // tweets.GetItemLinqQueryable<Tweet>().FirstOrDefault(t => t.Id == id);
                    // 👆は多分未対応
                    iterator = tweets.GetItemLinqQueryable<Tweet>()
                        .Where(t => t.Id == id && t.IsDeleted != true)
                        .ToFeedIterator();
                    result = await iterator.ReadNextAsync();
                    tweet = result.FirstOrDefault();

                    if (tweet == null)
                    {
                        // 対象のツイートがなければループを抜ける
                        break;
                    }

                    // スレッドに追加
                    thread.Add(tweet);

                    // 次に取得するツイートのIDを設定
                    id = tweet.ReplyTo;
                }

                if (thread.Any())
                {
                    // 投稿日時が古い順に並べる
                    thread = thread.OrderBy(t => t.CreateAt).ToList();
                    return new OkObjectResult(thread);
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        

        [FunctionName("UpdateTweetByUpdatedUserInfoTrigger")]
        public async Task UpdateTweetByUpdatedUserInfoTriggerAsync([QueueTrigger(AZURE_STORAGE_UPDATE_USER_INFO_IN_TWEET_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
        {
            try
            {
                if (myQueueItem == null)
                {
                    return;
                }

                var user = JsonSerializer.Deserialize<UpdateUserQueue>(myQueueItem).TwiHighUser;
                var patch = new[]
                {
                    PatchOperation.Set("/userDisplayId", user.DisplayId),
                    PatchOperation.Set("/userDisplayName", user.DisplayName),
                    PatchOperation.Set("/userAvatarUrl", user.AvatarUrl),
                    // あえてDateTimeOffset.UtcNowにしてる。タイムラインの更新時刻が同時刻であると。タイムライン取得時に50件を超過する。
                    PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow)
                };

                var query = new QueryDefinition("SELECT VALUE c.id FROM c ORDER BY c.createAt DESC");
                var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var iterator = tweets.GetItemQueryIterator<Guid>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(user.Id.ToString())
                });
                var batchTasks = new List<Task<ItemResponse<Tweet>>>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    foreach (var tweetId in response)
                    {
                        patch[3] = PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow);
                        var task = tweets.PatchItemAsync<Tweet>(
                            tweetId.ToString(),
                            new PartitionKey(user.Id.ToString()),
                            patch);
                        batchTasks.Add(task);
                    }
                }
                var batchResult = await Task.WhenAll(batchTasks);
                foreach (var result in batchResult)
                {
                    if ((int)result.StatusCode < 200 || 300 <= (int)result.StatusCode)
                    {
                        continue;
                    }

                    await QueueStorages.InsertMessageAsync(
                        AZURE_STORAGE_UPDATE_USER_INFO_IN_TIMELINE_QUEUE_NAME,
                        new UpdateTimelineQueue(result));
                }

                _logger.LogInformation("Batch finish. RU:{0}, Count:{1}, Success:{2}",
                    batchResult.Sum(r => r.Headers.RequestCharge),
                    batchResult.Length,
                    batchResult.LongCount(r => 200 <= (int)r.StatusCode && (int)r.StatusCode < 300));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex, ex.StackTrace);
                throw;
            }
        }
    }
}

