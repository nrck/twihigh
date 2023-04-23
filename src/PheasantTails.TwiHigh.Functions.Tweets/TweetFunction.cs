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
                _logger.LogInformation("Tweet���e�������J�n���܂��B");

                if (!req.TryGetUserId(out var id))
                {
                    _logger.LogWarning("���[�UID���擾�ł��܂���ł����B");
                    return new UnauthorizedResult();
                }

                var user = (await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME).ReadItemAsync<TwiHighUser>(id, new PartitionKey(id))).Resource;
                var context = await req.JsonDeserializeAsync<PostTweetContext>();
                var tweet = new Tweet
                {
                    Id = Guid.NewGuid(),
                    Text = context.Text,
                    ReplyTo = context.ReplyTo.TweetId,
                    UserId = user.Id,
                    UserDisplayId = user.DisplayId,
                    UserDisplayName = user.DisplayName,
                    UserAvatarUrl = user.AvatarUrl,
                    IsDeleted = false,
                    UpdateAt = DateTimeOffset.UtcNow,
                    CreateAt = DateTimeOffset.UtcNow
                };

                // �c�C�[�g�̍쐬
                var tweets = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME);
                var res = await tweets.CreateItemAsync(tweet);

                // ���v���C�悪����Ύ��s
                if (context.ReplyTo != null)
                {
                    // �N�G���̍쐬
                    //var query = new QueryDefinition(
                    //    "SELECT * FROM c " +
                    //    "WHERE c.id = @ReplyToId")
                    //    .WithParameter("@ReplyToId", context.ReplyTo.TweetId);
                    //var iterator = tweets.GetItemQueryIterator<Tweet>(query);
                    //if(iterator.HasMoreResults)
                    //{
                    //    var replyToTweet = (await iterator.ReadNextAsync()).Resource.FirstOrDefault();
                    //    replyToTweet.ReplyFrom = replyToTweet.ReplyFrom.Append(context.ReplyTo.Value).ToArray();
                    //    var patch = new[]
                    //    {       
                    //        PatchOperation.Add("/replyFrom", context.ReplyTo.Value)
                    //    };
                    //    tweets.PatchItemAsync()
                    //}
                    var patch = new[]
                    {
                        PatchOperation.Add("/replyFrom", res.Resource.Id)
                    };
                    var tweetid = context.ReplyTo.TweetId.ToString();
                    var key = new PartitionKey(context.ReplyTo.UserId.ToString());
                    await tweets.PatchItemAsync<Tweet>(tweetid, key, patch);
                    
                    // TimelineFunction�փL���[�𑗐M
                        // �������̓��v���C����X�V
                        // ���s���͎����̃c�C�[�g�̃��v���C��̍폜�X�V
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
                if (!req.TryGetUserId(out var userId))
                {
                    _logger.LogWarning("���[�UID���擾�ł��܂���ł����B");
                    return new UnauthorizedResult();
                }

                // �폜�Ώۂ̃c�C�[�g�ɍ폜�t���O�𗧂Ă�
                var patch = new[]
                {
                    // �폜�t���O
                    PatchOperation.Set("/isDeleted", true),
                    // �X�V���������ݎ�����
                    PatchOperation.Set("/updateAt", DateTimeOffset.Now)
                };

                // �����̃c�C�[�g�ȊO��ID���w�肳��Ă��A�����ŗ�O����������͂�
                try
                {
                    var tweet = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME)
                        .PatchItemAsync<Tweet>(id, new PartitionKey(userId), patch);

                    // �t�H�����[�̃^�C�����C������폜����
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

