using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.DataStore.Entity;
using PheasantTails.TwiHigh.Extensions;
using PheasantTails.TwiHigh.Model;
using PheasantTails.TwiHigh.Model.Timelines;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.FunctionCore.StaticStrings;

namespace PheasantTails.TwiHigh.TweetFunctions
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

        [FunctionName("Tweets")]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
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
                    UserId = user.Id.Value,
                    UserDisplayId = user.DisplayId,
                    UserDisplayName = user.DisplayName,
                    UserAvatarUrl = user.AvatarUrl,
                    CreateAt = DateTimeOffset.UtcNow
                };

                var res = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME).CreateItemAsync(tweet);
                var stringWritter = new MemoryStream();
                var queString = JsonSerializer.Serialize(new QueAddTimelineContext(tweet, user.Followers));
                await InsertMessageAsync(AZURE_STORAGE_ADD_TIMELINE_QUEUE_NAME, queString);
                return new CreatedResult("", res.Resource);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task InsertMessageAsync(string queueName, string message)
        {
            // Get the connection string from app settings
            var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

            // Instantiate a QueueClient which will be used to create and manipulate the queue
            var queueClient = new QueueClient(connectionString, queueName);

            // Create the queue if it doesn't already exist
            await queueClient.CreateIfNotExistsAsync();

            if (await queueClient.ExistsAsync())
            {
                // Send a message to the queue
                var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
                await queueClient.SendMessageAsync(base64str);
            }
        }
    }
}

