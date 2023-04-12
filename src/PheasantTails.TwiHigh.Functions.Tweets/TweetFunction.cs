using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Extensions;
using System;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets
{
    public class TweetFunction
    {
        private readonly ILogger<TweetFunction> _logger;
        private readonly CosmosClient _client;

        public TweetFunction(CosmosClient client, ILogger<TweetFunction> log)
        {
            _logger = log;
            _client = client;
        }

        [FunctionName("PostTweet")]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tweets")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Tweet投稿処理を開始します。");

                if (!req.TryGetUserId(out var id))
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
                    ReplyTo = context.ReplyTo,
                    UserId = user.Id,
                    UserDisplayId = user.DisplayId,
                    UserDisplayName = user.DisplayName,
                    UserAvatarUrl = user.AvatarUrl,
                    IsDeleted = false,
                    UpdateAt = DateTimeOffset.UtcNow,
                    CreateAt = DateTimeOffset.UtcNow
                };

                var res = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME).CreateItemAsync(tweet);
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
                if (!req.TryGetUserId(out var userId))
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
    }
}

