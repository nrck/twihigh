using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.Beta.Client.TypedHttpClients;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PheasantTails.TwiHigh.Beta.Client
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
            try
            {
                var token = await _localStorage.GetItemAsStringAsync(LOCAL_STORAGE_NAME_JWT);

                // tokenが無い
                if (string.IsNullOrWhiteSpace(token))
                {
                    return DefaultAuthenticationState;
                }

                // 有効期限の確認
                var exp = GetUtcExpiryFromJwt(token);
                if (exp.HasValue && exp.Value <= DateTime.UtcNow.AddDays(1))
                {
                    token = await GetRefreshedTokenAsync();
                    if (string.IsNullOrEmpty(token))
                    {
                        await _localStorage.RemoveItemAsync(LOCAL_STORAGE_NAME_JWT);
                        return DefaultAuthenticationState;
                    }
                    else
                    {
                        await _localStorage.SetItemAsync(LOCAL_STORAGE_NAME_JWT, token);
                    }
                }

                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "apiauth")));
            }
            catch (Exception)
            {
                return DefaultAuthenticationState;
            }
        }

        public async Task MarkUserAsAuthenticatedAsync(string token)
        {
            await _localStorage.SetItemAsStringAsync(LOCAL_STORAGE_NAME_JWT, token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "apiauth"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task RefreshAuthenticationStateAsync()
        {
            var token = await GetRefreshedTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                await MarkUserAsLoggedOutAsync();
            }
            else
            {
                await MarkUserAsAuthenticatedAsync(token);
            }
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await _localStorage.RemoveItemAsync(LOCAL_STORAGE_NAME_JWT);
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var jwtSecurityToken = DefaultJwtSecurityTokenHandler.ReadJwtToken(jwt);
            return jwtSecurityToken.Payload.Claims;
        }

        private DateTime? GetUtcExpiryFromJwt(string jwt)
        {
            var jwtSecurityToken = DefaultJwtSecurityTokenHandler.ReadJwtToken(jwt);
            if (jwtSecurityToken.Payload.Expiration.HasValue)
            {
                return DateTimeOffset.FromUnixTimeSeconds(jwtSecurityToken.Payload.Expiration.Value).UtcDateTime;
            }

            return null;
        }

        private async Task<string> GetRefreshedTokenAsync()
        {
            var savedToken = await _localStorage.GetItemAsStringAsync(LOCAL_STORAGE_NAME_JWT);

            // tokenが無い
            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return string.Empty;
            }

            // Refreshトークンの取得
            _httpClient.SetToken(savedToken);
            var newTokenRes = await _httpClient.RefreshAsync();

            return newTokenRes?.Token ?? string.Empty;
        }
    }
}
