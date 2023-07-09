using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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

        [FunctionName("PostTweet")]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tweets")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Tweet投稿処理を開始します。");

                if (!req.TryGetUserId(_tokenValidationParameters, out var id))
                {
                    _logger.LogWarning("ユーザIDを取得できませんでした。");
                    return new UnauthorizedResult();
                }

                var user = (await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME).ReadItemAsync<TwiHighUser>(id, new PartitionKey(id))).Resource;
                var context = await req.JsonDeserializeAsync<PostTweetContext>();
                var tweet = new Tweet
                {
                    Id = Guid.NewGuid(),
                    Text = context.Text,
                    ReplyTo = context.ReplyTo?.TweetId,
                    UserId = user.Id,
                    UserDisplayId = user.DisplayId,
                    UserDisplayName = user.DisplayName,
                    UserAvatarUrl = user.AvatarUrl,
                    IsDeleted = false,
                    UpdateAt = DateTimeOffset.UtcNow,
                    CreateAt = DateTimeOffset.UtcNow
                };

                // ツイートの作成
                var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var res = await tweets.CreateItemAsync(tweet);

                // リプライ先があれば実行
                if (context.ReplyTo != null)
                {
                    var patch = new[]
                    {
                        PatchOperation.Add("/replyFrom/-", tweet.Id),
                        PatchOperation.Set("/updateAt", DateTimeOffset.Now)
                    };
                    var tweetid = context.ReplyTo.TweetId.ToString();
                    var key = new PartitionKey(context.ReplyTo.UserId.ToString());
                    try
                    {
                        await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME).PatchItemAsync<Tweet>(tweetid, key, patch);

                        // 成功時はリプライ先を更新
                        await QueueStorages.InsertMessageAsync(
                            AZURE_STORAGE_UPDATE_REPLYFROM_TIMELINES_TWEET_TRIGGER_QUEUE_NAME,
                            new UpdateTimelineQueue(tweet));
                    }
                    catch (CosmosException ex)
                    {
                        _logger.LogError(ex, "PostTweet throw CosmosException.");
                        // 失敗時は自ツイートのreplyToを削除する
                        await QueueStorages.InsertMessageAsync(
                            AZURE_STORAGE_UPDATE_REPLYTO_TIMELINES_TWEET_TRIGGER_QUEUE_NAME,
                            new UpdateTimelineQueue(tweet));
                    }
                }
                await QueueStorages.InsertMessageAsync(
                    AZURE_STORAGE_ADD_TIMELINES_TWEET_TRIGGER_QUEUE_NAME,
                    new QueAddTimelineContext(tweet, user.Followers));
                return new CreatedResult("", res.Resource);
            }
            catch (Exception ex)
            {
                throw;
            }
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

        [FunctionName("GetTweetById")]
        public async Task<IActionResult> GetTweetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tweets/{tweetId}")] HttpRequest req,
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

        [FunctionName("GetUserTweet")]
        public async Task<IActionResult> GetUserTweetAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{userId}")] HttpRequest req,
            Guid userId)
        {
            try
            {
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

                var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var query = new QueryDefinition(
                    "SELECT TOP 50 * FROM c " +
                    "WHERE c.isDeleted != true " +
                    "AND (" +
                        "(@SinceDatetime < c.updateAt AND c.updateAt <= @UntilDatetime) " +
                        "OR (NOT IS_DEFINED(c.updateAt) " +
                        "AND (@SinceDatetime < c.createAt AND c.createAt <= @UntilDatetime))" +
                    ")" +
                    "ORDER BY c.createAt DESC")
                    .WithParameter("@SinceDatetime", sinceDatetime)
                    .WithParameter("@UntilDatetime", untilDatetime);

                var iterator = tweets.GetItemQueryIterator<Tweet>(query,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(userId.ToString())
                    });

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
    }
}

