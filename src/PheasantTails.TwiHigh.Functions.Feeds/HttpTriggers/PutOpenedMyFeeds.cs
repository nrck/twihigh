using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Data.Model.Feeds;
using PheasantTails.TwiHigh.Functions.Core.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Feeds.HttpTriggers;

public class PutOpenedMyFeeds
{
    private const string FUNCTION_NAME = "PutOpenedMyFeeds";
    private readonly CosmosClient _client;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly ILogger<PutOpenedMyFeeds> _logger;

    public PutOpenedMyFeeds(
        CosmosClient client,
        TokenValidationParameters tokenValidationParameters,
        ILogger<PutOpenedMyFeeds> logger)
    {
        _client = client;
        _tokenValidationParameters = tokenValidationParameters;
        _logger = logger;
    }

    [Function(FUNCTION_NAME)]
    public async Task<IActionResult> PutOpenedMyFeedsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "feeds")] HttpRequest req)
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

            // Get request body
            var context = await req.JsonDeserializeAsync<PutUpdateMyFeedsContext>();
            if (100 < context.Ids.Length)
            {
                _logger.TwiHighLogWarning(FUNCTION_NAME, "The request contains more than 100 IDs. The processing limit for this function is 100 IDs.");
                return new BadRequestObjectResult(context);
            }

            // Create patch operation
            var patch = new PatchOperation[]
            {
                PatchOperation.Set("/isOpened", true),
                PatchOperation.Set("/updateAt", DateTimeOffset.UtcNow)
            };

            // Execute batch
            TransactionalBatchResponse batchResult;
            try
            {
                var feedContainer = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_FEED_CONTAINER_NAME);
                var transactionalBatch = feedContainer.CreateTransactionalBatch(new PartitionKey(userId));
                foreach (var feedId in context.Ids)
                {
                    transactionalBatch.PatchItem(feedId.ToString(), patch);
                }
                batchResult = await transactionalBatch.ExecuteAsync();
            }
            catch (CosmosException ex)
            {
                throw new FeedException($"An error occurred while execute batch at feed container.", ex);
            }
            _logger.TwiHighLogInformation(FUNCTION_NAME, "Execute {0} feeds. RU: {1}", batchResult.Count, batchResult.RequestCharge);

            return new OkResult();
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
