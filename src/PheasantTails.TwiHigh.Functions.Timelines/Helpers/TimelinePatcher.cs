﻿using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Timelines.Helpers
{
    internal static class TimelinePatcher
    {
        internal static async Task<ResponseMessage[]> PatchTimelineAsync(this CosmosClient client, Guid tweetId, IReadOnlyList<PatchOperation> patch)
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
    }
}
