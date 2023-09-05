using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Functions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.HttpTriggers
{
    internal class GetMyTimeline
    {
        private const string FUNCTION_NAME = "GetMyTimeline";
        private const string QUERY_PARM_SINCE = "@SinceDatetime";
        private const string QUERY_PARM_UNTIL = "@UntilDatetime";
        private const string QUERY_PARM_MAXIMUM_TWEETS = "100";
        private readonly CosmosClient _client;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private static readonly QueryDefinition _queryDefinition = new($"""
            SELECT TOP {QUERY_PARM_MAXIMUM_TWEETS} * FROM c 
            WHERE ({QUERY_PARM_SINCE} < c.updateAt AND c.updateAt <= {QUERY_PARM_UNTIL}) 
            OR ({QUERY_PARM_SINCE} < c.createAt AND c.createAt <= {QUERY_PARM_UNTIL}) 
            ORDER BY c.createAt DESC
            """);

        public GetMyTimeline(CosmosClient client, TokenValidationParameters tokenValidationParameters)
        {
            _client = client;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task<IActionResult> GetMyTimelineAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "")] HttpRequest req,
           ILogger logger)
        {
            try
            {
                logger.TwiHighLogStart(FUNCTION_NAME);

                // Check authroized.
                if (!req.TryGetUserId(_tokenValidationParameters, out var userId))
                {
                    logger.TwiHighLogWarning(FUNCTION_NAME, "Cannot get a user id from JWT.");
                    return new UnauthorizedResult();
                }

                // Get datetime from query string.
                var sinceDatetime = req.GetSinceDatetime();
                var untilDatetime = req.GetUntilDatetime();
                logger.TwiHighLogInformation(FUNCTION_NAME, "Get user timeline. ID: {0}, From: {1}, To: {2}",
                    userId,
                    sinceDatetime,
                    untilDatetime);

                // Create querry.
                var query = _queryDefinition
                    .WithParameter(QUERY_PARM_SINCE, sinceDatetime)
                    .WithParameter(QUERY_PARM_UNTIL, untilDatetime);

                var tweets = new List<Tweet>();
                try
                {
                    var iterator = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME)
                        .GetItemQueryIterator<Timeline>(query, requestOptions: new QueryRequestOptions
                        {
                            PartitionKey = new PartitionKey(userId)
                        });

                    // Get tweets.
                    var requestCharge = 0.0;
                    while (iterator.HasMoreResults)
                    {
                        var res = await iterator.ReadNextAsync();
                        requestCharge += res.RequestCharge;
                        tweets.AddRange(res.Select(timeline => timeline.ToTweet()).ToArray());
                    }
                    logger.TwiHighLogInformation(FUNCTION_NAME, "Get {0} tweets. RU: {1}", tweets.Count, requestCharge);
                }
                catch (CosmosException ex)
                {
                    throw new TimelineException($"An error occurred while getting timeline items by a linq query.", ex);
                }

                if (!tweets.Any())
                {
                    return new NoContentResult();
                }

                // Create response context
                var latest = tweets.Max(t => t.CreateAt < t.UpdateAt ? t.UpdateAt : t.CreateAt);
                var oldest = tweets.Min(t => t.CreateAt > t.UpdateAt ? t.UpdateAt : t.CreateAt);
                var response = new ResponseTimelineContext
                {
                    Latest = latest,
                    Oldest = oldest,
                    Tweets = tweets.OrderByDescending(t => t.CreateAt).ToArray()
                };

                return new OkObjectResult(response);
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
