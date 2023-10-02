using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Tweets.HttpTriggers
{
    public class GetTweetsByUserId
    {
        private const string FUNCTION_NAME = "GetTweetsByUserId";
        private const string QUERY_PARM_SINCE = "@SinceDatetime";
        private const string QUERY_PARM_UNTIL = "@UntilDatetime";
        private readonly CosmosClient _client;
        private static readonly QueryDefinition _queryDefinition = new($"""
            SELECT TOP 50 * FROM c 
            WHERE c.isDeleted != true 
            AND {QUERY_PARM_SINCE} < c.updateAt 
            AND c.updateAt <= {QUERY_PARM_UNTIL} 
            ORDER BY c.createAt DESC
            """);

        public GetTweetsByUserId(CosmosClient client)
        {
            _client = client;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task<IActionResult> GetUserTweetsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tweets/user/{userId}")] HttpRequest req,
            Guid userId,
            ILogger logger)
        {
            try
            {
                logger.TwiHighLogStart(FUNCTION_NAME);

                // Get datetime from query string.
                var sinceDatetime = req.GetSinceDatetime();
                var untilDatetime = req.GetUntilDatetime();
                logger.TwiHighLogInformation(FUNCTION_NAME, "Get user tweets. ID: {0}, From: {1}, To: {2}",
                    userId,
                    sinceDatetime,
                    untilDatetime);

                // Create iterator
                var query = _queryDefinition.WithParameter(QUERY_PARM_SINCE, sinceDatetime)
                    .WithParameter(QUERY_PARM_UNTIL, untilDatetime);
                var iterator = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TWEET_CONTAINER_NAME)
                    .GetItemQueryIterator<Tweet>(query,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(userId.ToString())
                    });

                // Get tweets.
                var tweet = new List<Tweet>();
                var requestCharge = 0.0;
                while (iterator.HasMoreResults)
                {
                    var res = await iterator.ReadNextAsync();
                    requestCharge += res.RequestCharge;
                    tweet.AddRange(res);
                }

                // Return ActionResult.
                logger.TwiHighLogInformation(FUNCTION_NAME, "Get {0} tweets. RU: {1}", tweet.Count, requestCharge);
                if (tweet.Count > 0)
                {
                    return new OkObjectResult(tweet);
                }
                else
                {
                    return new NoContentResult();
                }
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
