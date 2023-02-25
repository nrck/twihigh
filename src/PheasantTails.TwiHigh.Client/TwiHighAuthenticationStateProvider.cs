using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace PheasantTails.TwiHigh.Client
{
    public class TwiHighAuthenticationStateProvider : AuthenticationStateProvider
    {
        private const string LOCAL_STORAGE_NAME_JWT = "TwiHighJwt";
        private readonly AppUserHttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        private AuthenticationState DefaultAuthenticationState { get; }
        private JwtSecurityTokenHandler DefaultJwtSecurityTokenHandler { get; }

        public TwiHighAuthenticationStateProvider(AppUserHttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            DefaultAuthenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            DefaultJwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _localStorage.GetItemAsStringAsync(LOCAL_STORAGE_NAME_JWT);

            // tokenが無い
            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return DefaultAuthenticationState;
            }

            // 有効期限の確認
            var exp = GetUtcExpiryFromJwt(savedToken);
            if (exp.HasValue && exp.Value <= DateTime.UtcNow.AddDays(1))
            {
                var newTokenRes = await _httpClient.RefreshAsync(savedToken);
                if (string.IsNullOrEmpty(newTokenRes?.Token))
                {
                    await _localStorage.RemoveItemAsync(LOCAL_STORAGE_NAME_JWT);
                    return DefaultAuthenticationState;
                }
                else
                {
                    await _localStorage.SetItemAsync(LOCAL_STORAGE_NAME_JWT, newTokenRes.Token);
                }
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "apiauth")));
        }

        public async Task MarkUserAsAuthenticatedAsync(string token)
        {
            await _localStorage.SetItemAsStringAsync(LOCAL_STORAGE_NAME_JWT, token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "apiauth"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await _localStorage.RemoveItemAsync(LOCAL_STORAGE_NAME_JWT);
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private Claim[] ParseClaimsFromJwt(string jwt)
        {
            var jwtSecurityToken = DefaultJwtSecurityTokenHandler.ReadJwtToken(jwt);
            if (jwtSecurityToken.Payload.TryGetValue(ClaimTypes.Role, out var roles))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                return parsedRoles?.Select(role => new Claim(ClaimTypes.Role, role)).ToArray() ?? Array.Empty<Claim>();
            }

            return Array.Empty<Claim>();
        }

        private DateTime? GetUtcExpiryFromJwt(string jwt)
        {
            var jwtSecurityToken = DefaultJwtSecurityTokenHandler.ReadJwtToken(jwt);
            if (jwtSecurityToken.Payload.TryGetValue("exp", out var exp) && long.TryParse(exp.ToString(), out long unixtime))
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixtime).UtcDateTime;
            }

            return null;
        }
    }
}
