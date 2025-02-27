using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Functions.Core.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Functions.Core.Queues;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Feeds.QueueTriggers;

public class InsertFavoredFeed
{
    private const string FUNCTION_NAME = "QueueTriggerInsertFavoredFeed";
    private readonly CosmosClient _client;
    private readonly ILogger<InsertFavoredFeed> _logger;

    public InsertFavoredFeed(
        CosmosClient client,
        ILogger<InsertFavoredFeed> logger)
    {
        _client = client;
        _logger = logger;
    }

    [Function(FUNCTION_NAME)]
    public async Task QueueTriggerInsertFavoredFeedAsync(
        [QueueTrigger(AZURE_STORAGE_FEED_FAVORED_BY_USER_QUEUE_NAME, Connection = QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME)] string myQueueItem)
    {
        try
        {
            _logger.TwiHighLogStart(FUNCTION_NAME);
            if (myQueueItem == null)
            {
                // null check
                throw new ArgumentNullException(nameof(myQueueItem), "Queue is Null");
            }
            var que = JsonSerializer.Deserialize<FeedFavoredQueue>(myQueueItem);

            // Get target tweet
            ItemResponse<Tweet> tweet;
            try
            {
                tweet = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME)
                    .ReadItemAsync<Tweet>(que.TargetTweetId.ToString(), new PartitionKey(que.TargetTweetPartitionKey));
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.TwiHighLogWarning(FUNCTION_NAME, "Target tweet is NOT found. ID: {0}", que.TargetTweetId);
                    return;
                }
                throw new FeedException($"An error occurred while retrieving the tweet. TweetID: {que.TargetTweetId}", ex);
            }
            _logger.TwiHighLogInformation(FUNCTION_NAME, "Target tweet is found. ID: {0}", tweet.Resource.Id);
            _logger.TwiHighLogInformation(FUNCTION_NAME, "{0} > {1}", tweet.Resource.UserDisplayId, tweet.Resource.Text);

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
                    _logger.TwiHighLogWarning(FUNCTION_NAME, "Feed by user is NOT found. ID: {0}", que.FeedByUserId);
                    return;
                }
                throw new FeedException($"An error occurred while retrieving the user. UserId: {que.FeedByUserId}", ex);
            }
            _logger.TwiHighLogInformation(FUNCTION_NAME, "Feed by user is found. ID: {0}", user.Resource.Id);
            _logger.TwiHighLogInformation(FUNCTION_NAME, "Feed by {0}", user.Resource.DisplayId);

            // Create a feed item.
            var feed = Feed.CreateFavored(tweet, user);
            var result = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_FEED_CONTAINER_NAME)
                .CreateItemAsync(feed);
            _logger.TwiHighLogInformation(FUNCTION_NAME, "Created Feed. Total RU: {0:0.00}", tweet.RequestCharge + user.RequestCharge + result.RequestCharge);
        }
        catch (Exception ex)
        {
            _logger.TwiHighLogError(FUNCTION_NAME, ex);
            throw;
        }
        finally
        {
            _logger.TwiHighLogEnd(FUNCTION_NAME);
        }
    }
}
