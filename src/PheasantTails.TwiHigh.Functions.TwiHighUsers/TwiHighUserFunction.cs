using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Data.Store.Entity;
using PheasantTails.TwiHigh.Functions.Core.Services;
using PheasantTails.TwiHigh.Functions.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

namespace PheasantTails.TwiHigh.Functions.TwiHighUsers
{
    public class TwiHighUserFunction
    {
        private readonly ILogger<TwiHighUserFunction> _logger;
        private readonly CosmosClient _client;
        private readonly IConfiguration _configuration;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IImageProcesserService _imageProcesserService;

        public TwiHighUserFunction(CosmosClient client,
            IConfiguration configuration,
            ILogger<TwiHighUserFunction> log,
            IAzureBlobStorageService azureBlobStorageService,
            IImageProcesserService imageProcesserService)
        {
            _logger = log;
            _client = client;
            _configuration = configuration;
            _azureBlobStorageService = azureBlobStorageService;
            _imageProcesserService = imageProcesserService;
        }

        [FunctionName("SignUp")]
        public async Task<IActionResult> SignUpAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var context = await req.JsonDeserializeAsync<AddTwiHighUserContext>();

            // DisplayIdがnullか空白ならエラー
            if (string.IsNullOrWhiteSpace(context.DisplayId))
            {
                return new BadRequestObjectResult(context);
            }

            // 重複確認
            if (await IsExistDisplayIdAsync(context.DisplayId))
            {
                return new ConflictObjectResult(context);
            }

            // ユーザの作成
            var user = new TwiHighUser
            {
                Id = Guid.NewGuid(),
                DisplayId = context.DisplayId,
                LowerDisplayId = context.DisplayId.ToLower(),
                DisplayName = context.DisplayName,
                Biography = string.Empty,
                Email = context.Email,
                Followers = Array.Empty<Guid>(),
                Follows = Array.Empty<Guid>(),
                CreateAt = DateTimeOffset.UtcNow,
                AvatarUrl = "https://twihighdevstorageaccount.blob.core.windows.net/twihigh-images/pengin.jpeg"
            };
            user.HashedPassword = new PasswordHasher<TwiHighUser>().HashPassword(user, context.Password);

            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);
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
            var query = new QueryDefinition("SELECT * FROM c WHERE c.lowerDisplayId = @displayId OFFSET 0 LIMIT 1")
                .WithParameter("@displayId", context.DisplayId.ToLower());

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

            var claims = GenerateClaims(user);
            var token = GenerateJwt(claims);
            var jwt = new ResponseJwtContext
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return new OkObjectResult(jwt);
        }

        [FunctionName("TwiHighUser")]
        public async Task<IActionResult> TwiHighUserAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TwiHighUser/{id}")] HttpRequest req,
            string id
            )
        {
            var user = await GetTwiHighUserByIdOrDisplayIdAsync(id);
            if (user == null)
            {
                return new NotFoundResult();
            }
            var response = new ResponseTwiHighUserContext(user);
            return new OkObjectResult(response);
        }

        [FunctionName("TwiHighUserFollows")]
        public async Task<IActionResult> TwiHighUserFollowsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TwiHighUser/{id}/Follows")] HttpRequest req,
            string id
            )
        {
            var user = await GetTwiHighUserByIdOrDisplayIdAsync(id);
            if (user == null)
            {
                return new NotFoundResult();
            }

            if (user.Follows.Length == 0)
            {
                return new OkObjectResult(Array.Empty<ResponseTwiHighUserContext>());
            }

            var follows = (await GetTwiHighUsersAsync(user.Follows))
                .Select(u => new ResponseTwiHighUserContext(u))
                .ToArray();

            return new OkObjectResult(follows);
        }

        [FunctionName("TwiHighUserFollowers")]
        public async Task<IActionResult> TwiHighUsersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TwiHighUser/{id}/Followers")] HttpRequest req,
            string id
            )
        {
            var user = await GetTwiHighUserByIdOrDisplayIdAsync(id);
            if (user == null)
            {
                return new NotFoundResult();
            }

            if (user.Followers.Length == 0)
            {
                return new OkObjectResult(Array.Empty<ResponseTwiHighUserContext>());
            }

            var followers = (await GetTwiHighUsersAsync(user.Followers))
                .Select(u => new ResponseTwiHighUserContext(u))
                .ToArray();

            return new OkObjectResult(followers);
        }

        [FunctionName("Refresh")]
        public async Task<IActionResult> RefreshAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
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
            var claims = GenerateClaims(user);
            var token = GenerateJwt(claims);
            var jwt = new ResponseJwtContext
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return new OkObjectResult(jwt);
        }

        [FunctionName("PatchTwiHighUser")]
        public async Task<IActionResult> PatchTwiHighUserAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "TwiHighUser")] HttpRequest req)
        {
            try
            {
                if (!req.TryGetUserId(out var id))
                {
                    return new UnauthorizedResult();
                }
                PatchTwiHighUserContext patch;
                try
                {
                    patch = await req.JsonDeserializeAsync<PatchTwiHighUserContext>();
                }
                catch (Exception)
                {
                    return new BadRequestResult();
                }

                var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);
                var user = await users.ReadItemAsync<TwiHighUser>(id, new PartitionKey(id));
                if (!string.IsNullOrWhiteSpace(patch.Password))
                {
                    patch.Password = new PasswordHasher<TwiHighUser>().HashPassword(user, patch.Password);
                }

                if (!string.IsNullOrWhiteSpace(patch.DisplayId) && await IsExistDisplayIdAsync(patch.DisplayId))
                {
                    return new ConflictResult();
                }

                var operations = await GetPatchOperationsAsync(patch);
                if (!operations.Any())
                {
                    return new BadRequestObjectResult(patch);
                }

                TwiHighUser result = await users.PatchItemAsync<TwiHighUser>(id, new PartitionKey(id), operations, requestOptions: new PatchItemRequestOptions { IfMatchEtag = user.ETag });
                return new OkObjectResult(new ResponseTwiHighUserContext(result));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception happend at {0}", ex.Source);
                throw;
            }
        }

        private List<Claim> GenerateClaims(TwiHighUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(nameof(ResponseTwiHighUserContext.Id), user.Id.ToString()),
                new Claim(nameof(ResponseTwiHighUserContext.DisplayId), user.DisplayId),
                new Claim(nameof(ResponseTwiHighUserContext.DisplayName), user.DisplayName),
                new Claim(nameof(ResponseTwiHighUserContext.AvatarUrl), user.AvatarUrl)
            };

            return claims;
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

        private async Task<TwiHighUser> GetTwiHighUserByIdOrDisplayIdAsync(string id)
        {
            TwiHighUser user;
            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);
            if (Guid.TryParse(id, out var _))
            {
                var res = await users.ReadItemAsync<TwiHighUser>(id, new PartitionKey(id));
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                user = res.Resource;
            }
            else
            {
                // クエリの作成
                var query = new QueryDefinition("SELECT * FROM c WHERE c.lowerDisplayId = @displayId OFFSET 0 LIMIT 1")
                    .WithParameter("@displayId", id.ToLower());
                var iterator = users.GetItemQueryIterator<TwiHighUser>(query);
                var res = await iterator.ReadNextAsync();
                if (res.StatusCode != HttpStatusCode.OK || !res.Any())
                {
                    return null;
                }
                user = res.Resource.First();
            }

            return user;
        }

        private async Task<TwiHighUser[]> GetTwiHighUsersAsync(Guid[] ids)
        {
            var items = ids.Select(id => (id.ToString(), new PartitionKey(id.ToString()))).ToArray();
            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);
            var result = await users.ReadManyItemsAsync<TwiHighUser>(items);

            return result.ToArray();
        }

        private async Task<List<PatchOperation>> GetPatchOperationsAsync(PatchTwiHighUserContext context)
        {
            var operations = new List<PatchOperation>();
            if (!string.IsNullOrWhiteSpace(context.DisplayId))
            {
                operations.Add(PatchOperation.Set("/displayId", context.DisplayId));
                operations.Add(PatchOperation.Set("/lowerDisplayId", context.DisplayId.ToLower()));
                _logger.LogInformation("Patch:: {0}: {1}", nameof(context.DisplayId), context.DisplayId);
            }
            if (!string.IsNullOrWhiteSpace(context.DisplayName))
            {
                operations.Add(PatchOperation.Set("/displayName", context.DisplayName));
                _logger.LogInformation("Patch:: {0}: {1}", nameof(context.DisplayName), context.DisplayName);
            }
            if (!string.IsNullOrWhiteSpace(context.Password))
            {
                operations.Add(PatchOperation.Set("/hashedPassword", context.Password));
                _logger.LogInformation("Patch:: {0}: {1}", nameof(context.Password), context.Password);
            }
            if (!string.IsNullOrWhiteSpace(context.Email))
            {
                operations.Add(PatchOperation.Set("/email", context.Email));
                _logger.LogInformation("Patch:: {0}: {1}", nameof(context.Email), context.Email);
            }
            if (!string.IsNullOrEmpty(context.Biography))
            {
                operations.Add(PatchOperation.Set("/biography", context.Biography));
                _logger.LogInformation("Patch:: {0}: {1}", nameof(context.Biography), context.Biography);
            }
            if (context.Base64EncodedAvatarImage != null)
            {
                // アップロード処理
                var rawData = context.DecodeAvaterImage();
                var filetype = context.Base64EncodedAvatarImage.ContentType.Split("/")[1];
                var format = filetype == "png" ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
                var data = _imageProcesserService.TrimmingToSquare(rawData, format);
                var url = await _azureBlobStorageService.UploadAsync(
                    "twihigh-images", $"icon/{Guid.NewGuid()}.{filetype}", new BinaryData(data));
                operations.Add(PatchOperation.Set("/avatarUrl", url.OriginalString));
            }

            return operations;
        }

        private async Task<bool> IsExistDisplayIdAsync(string displayId)
        {
            // クエリの作成
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.lowerDisplayId = @displayId")
                .WithParameter("@displayId", displayId.ToLower());

            // 重複確認
            var users = _client.GetContainer(TWIHIGH_COSMOSDB_NAME, TWIHIGH_USER_CONTAINER_NAME);
            var iterator = users.GetItemQueryIterator<long>(query);
            long count = 0;
            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();
                count += result.Resource.Sum();
            }

            return 0 < count;
        }
    }
}
