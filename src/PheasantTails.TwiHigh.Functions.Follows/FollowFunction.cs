using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.Data.Model.Followers;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.Follows
{
    public class FollowFunction
    {
        private readonly ILogger<FollowFunction> _logger;
        private readonly CosmosClient _client;

        public FollowFunction(CosmosClient client, ILogger<FollowFunction> log)
        {
            _logger = log;
            _client = client;
        }

        [FunctionName("AddFollow")]
        public async Task<IActionResult> AddFollow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "{followeeUserId}")] HttpRequest req,
            string followeeUserId)
        {
            if (!req.TryGetUserId(out var followerUserId))
            {
                return new UnauthorizedResult();
            }

            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);

            var followee = await users.ReadItemAsync<TwiHighUser>(followeeUserId, new PartitionKey(followeeUserId));
            if (followee.StatusCode != HttpStatusCode.OK)
            {
                return new NotFoundResult();
            }

            var follower = await users.ReadItemAsync<TwiHighUser>(followerUserId, new PartitionKey(followerUserId));
            if (follower.StatusCode != HttpStatusCode.OK)
            {
                return new BadRequestResult();
            }

            followee.Resource.Followers = followee.Resource.Followers.Append(follower.Resource.Id).ToArray();
            follower.Resource.Follows = follower.Resource.Follows.Append(followee.Resource.Id).ToArray();

            await users.UpsertItemAsync(followee.Resource);
            await users.UpsertItemAsync(follower.Resource);

            await QueueStorages.InsertMessageAsync(
                    AZURE_STORAGE_ADD_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME,
                    new AddNewFolloweeTweetContext { FolloweeId = followee.Resource.Id, UserId = follower.Resource.Id });

            return new NoContentResult();
        }

        [FunctionName("RemoveFollow")]
        public async Task<IActionResult> RemoveFollow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "{followeeUserId}")] HttpRequest req,
            string followeeUserId)
        {
            if (!req.TryGetUserId(out var followerUserId))
            {
                return new UnauthorizedResult();
            }

            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);

            var followee = await users.ReadItemAsync<TwiHighUser>(followeeUserId, new PartitionKey(followeeUserId));
            if (followee.StatusCode != HttpStatusCode.OK)
            {
                return new NotFoundResult();
            }

            var follower = await users.ReadItemAsync<TwiHighUser>(followerUserId, new PartitionKey(followerUserId));
            if (follower.StatusCode != HttpStatusCode.OK)
            {
                return new BadRequestResult();
            }

            var followeeIndex = Array.FindIndex(followee.Resource.Followers, (id) => id == follower.Resource.Id);
            if (0 <= followeeIndex)
            {
                var patch = new[]
                {
                    PatchOperation.Remove($"/followers/{followeeIndex}"),
                    PatchOperation.Set("/updateAt", DateTimeOffset.Now)
                };
                await users.PatchItemAsync<Tweet>(followeeUserId, new PartitionKey(followeeUserId), patch);
            }

            var followerIndex = Array.FindIndex(follower.Resource.Follows, (id) => id == followee.Resource.Id);
            if (0 <= followerIndex)
            {
                var patch = new[]
                {
                    PatchOperation.Remove($"/follows/{followerIndex}"),
                    PatchOperation.Set("/updateAt", DateTimeOffset.Now)
                };
                await users.PatchItemAsync<Tweet>(followerUserId, new PartitionKey(followerUserId), patch);
            }

            await QueueStorages.InsertMessageAsync(
                    AZURE_STORAGE_DELETE_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME,
                    new RemoveFolloweeTweetContext { FolloweeId = followee.Resource.Id, UserId = follower.Resource.Id });

            return new NoContentResult();
        }
    }
}
