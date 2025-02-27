using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Functions.Core.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Functions.Feeds.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Feeds.HttpTriggers;

public class GetMyFeeds
{
    private const string FUNCTION_NAME = "GetMyFeeds";
    private const int GET_FEED_MAX_LENGTH = 1000;
    private const string QUERY_PARM_SINCE = "@SinceDatetime";
    private const string QUERY_PARM_UNTIL = "@UntilDatetime";
    private readonly CosmosClient _client;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly ILogger<GetMyFeeds> _logger;
    private static readonly QueryDefinition _queryDefinition = new($"""
        SELECT TOP {GET_FEED_MAX_LENGTH} * FROM c
        WHERE {QUERY_PARM_SINCE} < c.updateAt
        AND c.updateAt <= {QUERY_PARM_UNTIL}
        ORDER BY c.createAt DESC
        """);

    public GetMyFeeds(
        CosmosClient client,
        TokenValidationParameters tokenValidationParameters,
        ILogger<GetMyFeeds> logger)
    {
        _client = client;
        _tokenValidationParameters = tokenValidationParameters;
        _logger = logger;
    }

    [Function(FUNCTION_NAME)]
    public async Task<IActionResult> GetMyFeedsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feeds")] HttpRequest req)
    {
        try
        {
            _logger.TwiHighLogStart(FUNCTION_NAME);

            // Check authroized.
            if (!req.TryGetUserId(_tokenValidationParameters, out var userId))
            {
                _logger.TwiHighLogWarning(FUNCTION_NAME, "Cannot get a user id from JWT.");
                return new UnauthorizedResult();
            }

            // Get the user from cosmos db.
            ItemResponse<TwiHighUser> userReadResponse;
            try
            {
                userReadResponse = await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME)
                    .ReadItemAsync<TwiHighUser>(userId, new PartitionKey(userId));
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.TwiHighLogWarning(FUNCTION_NAME, "Authroized user is NOT found. ID: {0}", userId);
                    return new UnauthorizedResult();
                }
                throw new FeedException($"An error occurred while retrieving the user. UserId: {userId}", ex);
            }
            _logger.TwiHighLogInformation(FUNCTION_NAME, "Authroized user is found. ID: {0}", userReadResponse.Resource.Id);

            // Get datetime from query string.
            var sinceDatetime = req.GetSinceDatetime();
            var untilDatetime = req.GetUntilDatetime();
            _logger.TwiHighLogInformation(FUNCTION_NAME, "Get feeds by {0}. From: {1}, To: {2}",
                userReadResponse.Resource.DisplayId,
                sinceDatetime,
                untilDatetime);

            // Create querry.
            var query = _queryDefinition
                .WithParameter(QUERY_PARM_SINCE, sinceDatetime)
                .WithParameter(QUERY_PARM_UNTIL, untilDatetime);

            var feeds = new List<Feed>();
            try
            {
                var iterator = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_FEED_CONTAINER_NAME)
                    .GetItemQueryIterator<Feed>(query, requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(userId)
                    });

                // Get tweets.
                var requestCharge = 0.0;
                while (iterator.HasMoreResults)
                {
                    var res = await iterator.ReadNextAsync();
                    requestCharge += res.RequestCharge;
                    feeds.AddRange(res);
                }
                _logger.TwiHighLogInformation(FUNCTION_NAME, "Get {0} feeds. RU: {1}", feeds.Count, requestCharge);
            }
            catch (CosmosException ex)
            {
                throw new FeedException($"An error occurred while getting feed items.", ex);
            }

            if (!feeds.Any())
            {
                return new NoContentResult();
            }

            // Get tweets
            var tweetIds = feeds.Where(f => f.FeedToTweetId.HasValue || f.FeedByTweetId.HasValue)
                .SelectMany(f =>
                {
                    var list = new List<(string id, PartitionKey partitionKey)>();
                    if (f.FeedToTweetId.HasValue)
                    {
                        list.Add((f.FeedToTweetId.ToString(), new PartitionKey(f.FeedToUserId.ToString())));
                    }
                    if (f.FeedByTweetId.HasValue)
                    {
                        list.Add((f.FeedByTweetId.ToString(), new PartitionKey(f.FeedByUserId.ToString())));
                    }

                    return list;
                })
                .ToArray();
            var tweets = tweetIds.Any()
                ? await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME).ReadManyItemsAsync<Tweet>(tweetIds)
                : default;

            // Get users
            var userIds = feeds.Where(f => f.FeedByUserId.HasValue)
                .Select(f => (id: f.FeedByUserId.ToString(), partitionKey: new PartitionKey(f.FeedByUserId.ToString())))
                .ToArray();
            var users = userIds.Any()
                ? await _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME).ReadManyItemsAsync<TwiHighUser>(userIds)
                : default;

            // Create response context
            var response = feeds.Select(f => FeedEntityHelper.CreateFeedContext(
                f,
                tweets.FirstOrDefault(t => t.Id == f.FeedToTweetId),
                users.FirstOrDefault(u => u.Id == f.FeedByUserId),
                tweets.FirstOrDefault(t => t.Id == f.FeedByTweetId)))
                .ToResponseFeedsContext();
            _logger.TwiHighLogInformation(FUNCTION_NAME, "Tweets RU: {0}, Users RU: {1}", tweets.RequestCharge, users.RequestCharge);

            return new OkObjectResult(response);
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
