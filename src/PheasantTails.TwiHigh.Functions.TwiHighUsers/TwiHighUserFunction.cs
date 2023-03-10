using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.DataStore.Entity;
using PheasantTails.TwiHigh.Extensions;
using PheasantTails.TwiHigh.Model.TwiHighUsers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.FunctionCore.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.TwiHighUsers
{
    public class TwiHighUserFunction
    {
        private readonly ILogger<TwiHighUserFunction> _logger;
        private readonly CosmosClient _client;
        private readonly IConfiguration _configuration;

        public TwiHighUserFunction(CosmosClient client, IConfiguration configuration, ILogger<TwiHighUserFunction> log)
        {
            _logger = log;
            _client = client;
            _configuration = configuration;
        }

        [FunctionName("SignInAppUser")]
        public async Task<IActionResult> SignInAppUserAsync(
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
            while (iterator.HasMoreResults)
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

        [FunctionName("Login")]
        public async Task<IActionResult> LoginAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var context = await req.JsonDeserializeAsync<PostAuthorizationContext>();

            // DisplayIdがnullか空白ならエラー
            if (string.IsNullOrWhiteSpace(context.DisplayId) || string.IsNullOrWhiteSpace(context.PlanePassword))
            {
                return new BadRequestObjectResult(context);
            }

            // クエリの作成
            var query = new QueryDefinition("SELECT * FROM c WHERE c.displayId = @displayId OFFSET 0 LIMIT 1")
                .WithParameter("@displayId", context.DisplayId);

            // 重複確認
            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);
            var iterator = users.GetItemQueryIterator<TwiHighUser>(query);
            FeedResponse<TwiHighUser> result = null;
            while (iterator.HasMoreResults)
            {
                result = await iterator.ReadNextAsync();
                if (1 < result.Count)
                {
                    return new BadRequestResult();
                }
            }
            if (result == null || 0 == result.Count)
            {
                return new NotFoundResult();
            }

            var user = result.First();
            var passwordVerificationResult = new PasswordHasher<TwiHighUser>().VerifyHashedPassword(user, user.HashedPassword, context.PlanePassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return new BadRequestObjectResult(context);
            }

            var token = GenerateJwt(new Claim[] { new Claim("id", user.Id.ToString()) });
            var jwt = new ResponseJwtContext
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return new OkObjectResult(jwt);
        }

        [FunctionName("Refresh")]
        public async Task<IActionResult> RefreshAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            if (req.TryGetUserId(out var id))
            {
                return new UnauthorizedResult();
            }

            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);
            var result = await users.ReadItemAsync<TwiHighUser>(id, new PartitionKey(id));
            if (result.StatusCode != HttpStatusCode.OK)
            {
                return new NotFoundResult();
            }
            var user = result.Resource;
            var token = GenerateJwt(new Claim[] { new Claim("id", user.Id.ToString()) });
            var jwt = new ResponseJwtContext
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return new OkObjectResult(jwt);
        }


        private JwtSecurityToken GenerateJwt(IEnumerable<Claim> claims = null)
        {
            // JWTの元ネタ
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

            // JWTの作成
            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtAudience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            return token;
        }
    }
}
