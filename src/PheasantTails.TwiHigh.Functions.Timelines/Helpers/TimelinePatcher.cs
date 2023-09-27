using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using PheasantTails.TwiHigh.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.Helpers
{
    internal static class TimelinePatcher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="TimelineException"></exception>
        /// <param name="client"></param>
        /// <param name="tweetId"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        /// <exception cref="TimelineException"></exception>
        internal static async Task<ResponseMessage[]> PatchTimelineAsync(this CosmosClient client, Guid tweetId, IReadOnlyList<PatchOperation> patch)
        {
            try
            {
                var timelines = client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);

                // クエリの作成
                var query = new QueryDefinition(
                    "SELECT c.id, c.ownerUserId FROM c " +
                    "WHERE c.tweetId = @TweetId")
                    .WithParameter("@TweetId", tweetId);

                var iterator = timelines.GetItemQueryIterator<TimelineIdOwnerUserIdPair>(query);

                var tasks = new List<Task<ResponseMessage>>();
                while (iterator.HasMoreResults)
                {
                    var result = await iterator.ReadNextAsync();
                    foreach (var pair in result)
                    {
                        tasks.Add(timelines
                            .PatchItemStreamAsync(
                                id: pair.Id.ToString(),
                                partitionKey: new PartitionKey(pair.OwnerUserId.ToString()),
                                patchOperations: patch,
                                requestOptions: new PatchItemRequestOptions { IfMatchEtag = result.ETag })
                            );
                    }
                }
                var batchResult = await Task.WhenAll(tasks);

                return batchResult;
            }
            catch (Exception ex)
            {
                throw new TimelineException($"Unhandled exception is happen at {nameof(PatchTimelineAsync)}().", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="TimelineException"></exception>
        /// <param name="client"></param>
        /// <param name="tweetId"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        /// <exception cref="TimelineException"></exception>
        internal static async Task PatchTimelineAsync<T>(this CosmosClient client, string myQueueItem, string functionName, ILogger logger) where T : ITimelinePatchOperationable
        {
            try
            {
                logger.TwiHighLogStart(functionName);
                if (string.IsNullOrEmpty(myQueueItem))
                {
                    // null check
                    throw new ArgumentNullException(nameof(myQueueItem), "Queue is Null");
                }

                var que = JsonSerializer.Deserialize<T>(myQueueItem);

                var timelines = client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_TIMELINE_CONTAINER_NAME);

                // クエリの作成
                var query = new QueryDefinition(
                    "SELECT c.id, c.ownerUserId FROM c " +
                    "WHERE c.tweetId = @TweetId")
                    .WithParameter("@TweetId", que.TweetId);
                var operations = que.GetPatchOperations();

                var iterator = timelines.GetItemQueryIterator<TimelineIdOwnerUserIdPair>(query);

                var tasks = new List<Task<ResponseMessage>>();
                while (iterator.HasMoreResults)
                {
                    var result = await iterator.ReadNextAsync();
                    foreach (var pair in result)
                    {
                        tasks.Add(timelines
                            .PatchItemStreamAsync(
                                id: pair.Id.ToString(),
                                partitionKey: new PartitionKey(pair.OwnerUserId.ToString()),
                                patchOperations: operations,
                                requestOptions: new PatchItemRequestOptions { IfMatchEtag = result.ETag })
                            );
                    }
                }
                var batchResult = await Task.WhenAll(tasks);

                logger.TwiHighLogInformation(functionName, "{0} batch finish. RU:{1}, Task Count:{2}, Success:{3}",
                    nameof(PatchTimelineAsync),
                    batchResult.Sum(r => r.Headers.RequestCharge),
                    batchResult.LongLength,
                    batchResult.LongCount(r => r.IsSuccessStatusCode));
            }
            catch (Exception ex)
            {
                logger.TwiHighLogError(functionName, ex);
                throw;
            }
            finally
            {
                logger.TwiHighLogEnd(functionName);
            }
        }
    }
}
