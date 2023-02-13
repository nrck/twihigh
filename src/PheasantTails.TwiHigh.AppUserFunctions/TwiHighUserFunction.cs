using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PheasantTails.TwiHigh.DataStore.Entity;
using PheasantTails.TwiHigh.Extensions;
using PheasantTails.TwiHigh.Model.TwiHighUsers;
using System;
using System.Linq;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.FunctionCore.StaticStrings;

namespace PheasantTails.TwiHigh.AppUserFunctions
{
    public class TwiHighUserFunction
    {
        private readonly ILogger<TwiHighUserFunction> _logger;
        private readonly CosmosClient _client;

        public TwiHighUserFunction(CosmosClient client, ILogger<TwiHighUserFunction> log)
        {
            _logger = log;
            _client = client;
        }

        [FunctionName("AddAppUser")]
        public async Task<IActionResult> AddAppUserAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var context = await req.JsonDeserializeAsync<AddTwiHighUserContext>();

            // DisplayIdがnullか空白ならエラー
            if (string.IsNullOrWhiteSpace(context.DisplayId))
            {
                return new BadRequestObjectResult(context);
            }

            // クエリの作成
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.displayId = @displayId")
                .WithParameter("@displayId", context.DisplayId);

            // 重複確認
            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);
            var iterator = users.GetItemQueryIterator<long>(query);
            long count = 0;
            while(iterator.HasMoreResults) 
            {
                var result = await iterator.ReadNextAsync();
                count += result.Resource.Sum();
            }
            if (0 < count)
            {
                return new ConflictObjectResult(context);
            }

            // ユーザの作成
            var user = new TwiHighUser
            {
                Id = Guid.NewGuid(),
                DisplayId = context.DisplayId,
                DisplayName = context.DisplayName,
                Biography = string.Empty,
                Email = context.Email,
                Followers = Array.Empty<Guid>(),
                Follows = Array.Empty<Guid>(),
                CreateAt = DateTimeOffset.UtcNow,
                AvatarUrl = string.Empty
            };
            user.HashedPassword = new PasswordHasher<TwiHighUser>().HashPassword(user, context.Password);

            var created = await users.CreateItemAsync(user);

            return new CreatedResult("", created.Resource);
        }
    }
}
