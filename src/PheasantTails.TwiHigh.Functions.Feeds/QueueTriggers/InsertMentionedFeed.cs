using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Queues;
using PheasantTails.TwiHigh.Functions.Core.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Feeds.QueueTriggers
{
    public class InsertMentionedFeed
    {
        private const string FUNCTION_NAME = "QueueTriggerInsertMentionedFeed";
        private readonly CosmosClient _client;

        public InsertMentionedFeed(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task QueueTriggerInsertMentionedFeedAsync(
            [QueueTrigger(AZURE_STORAGE_FEED_MENTIONED_BY_USER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem,
            ILogger logger)
        {
            try
            {
                logger.TwiHighLogStart(FUNCTION_NAME);
                if (myQueueItem == null)
                {
                    // null check
                    throw new ArgumentNullException(nameof(myQueueItem), "Queue is Null");
                }
                var que = JsonSerializer.Deserialize<FeedMentionedQueue>(myQueueItem);

                // Get target tweet
                FeedResponse<Tweet> targetAndReplyFromTweets;
                try
                {
                    targetAndReplyFromTweets = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME)
                        .ReadManyItemsAsync<Tweet>(new List<(string, PartitionKey)>{
                            (que.TargetTweetId.ToString(), new PartitionKey(que.TargetTweetPartitionKey)),
                            (que.FeedByTweetId.ToString(), new PartitionKey(que.FeedByTweetPartitionKey))
                        });
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, "Target tweet is NOT found. ID: {0}, {1}", que.TargetTweetId, que.FeedByTweetId);
                        return;
                    }
                    throw new FeedException($"An error occurred while retrieving the tweet. TweetID: {que.TargetTweetId}, {que.FeedByTweetId}", ex);
                }
                var targetTweet = targetAndReplyFromTweets.Resource.FirstOrDefault(t => t.Id == que.TargetTweetId);
                var feedByTweet = targetAndReplyFromTweets.Resource.FirstOrDefault(t => t.Id == que.FeedByTweetId);
                logger.TwiHighLogInformation(FUNCTION_NAME, "Target tweet is found. ID: {0}", targetTweet.Id);
                logger.TwiHighLogInformation(FUNCTION_NAME, "{0} > {1}", targetTweet.UserDisplayId, targetTweet.Text);
                logger.TwiHighLogInformation(FUNCTION_NAME, "Reply from tweet is found. ID: {0}", feedByTweet.Id);
                logger.TwiHighLogInformation(FUNCTION_NAME, "{0} > {1}", feedByTweet.UserDisplayId, feedByTweet.Text);

                // Get target user
                ItemResponse<TwiHighUser> user;
                try
                {
                    user = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME)
                        .ReadItemAsync<TwiHighUser>(que.FeedByUserId.ToString(), new PartitionKey(que.FeedByUserPartitionKey));
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        logger.TwiHighLogWarning(FUNCTION_NAME, "Feed by user is NOT found. ID: {0}", que.FeedByUserId);
                        return;
                    }
                    throw new FeedException($"An error occurred while retrieving the user. UserId: {que.FeedByUserId}", ex);
                }
                logger.TwiHighLogInformation(FUNCTION_NAME, "Feed by user is found. ID: {0}", user.Resource.Id);
                logger.TwiHighLogInformation(FUNCTION_NAME, "Feed by {0}", user.Resource.DisplayId);

                // Create a feed item.
                var feed = Feed.CreateMentioned(targetTweet, user, feedByTweet);
                var result = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_FEED_CONTAINER_NAME)
                    .CreateItemAsync(feed);
                logger.TwiHighLogInformation(FUNCTION_NAME, "Created Feed. Total RU: {0:0.00}", targetAndReplyFromTweets.RequestCharge + user.RequestCharge + result.RequestCharge);
            }
            catch (Exception ex)
            {
                logger.TwiHighLogError(FUNCTION_NAME, ex);
                throw;
            }
            finally
            {
                logger.TwiHighLogEnd(FUNCTION_NAME);
            }
        }
    }
}
