using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public class TwiHighAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string LOCAL_STORAGE_NAME_JWT = "TwiHighJwt";
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationState _defaultAuthenticationState;
    private readonly JwtSecurityTokenHandler _defaultJwtSecurityTokenHandler;
    private readonly string _apiUrlRefreshToken;

    public TwiHighAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _apiUrlRefreshToken = $"{configuration["AppUserApiUrl"]}/Refresh";
        _defaultAuthenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        _defaultJwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var jwt = await GetTokenFromLocalStorageAsync();

        // Check Expiry.
        var newjwt = await EnsureWithinExpirationAsync(jwt, TimeSpan.FromDays(1));
        if (string.IsNullOrEmpty(newjwt))
        {
            // If this token is null, Remove token value from local storage.
            await RemoveTokenFromLocalStorageAsync();
            return _defaultAuthenticationState;
        }

        // Create AuthenticationState object.
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(newjwt), "apiauth")));

        // Update token.
        if (jwt != newjwt)
        {
            await SetTokenToLocalStorageAsync(newjwt);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        return authState;
    }

    /// <summary>
    /// Mark the user as authenticated.
    /// </summary>
    /// <param name="jwt">Bearer token from the server.</param>
    public async ValueTask MarkUserAsAuthenticatedAsync(string jwt, CancellationToken cancellationToken = default)
    {
        await SetTokenToLocalStorageAsync(jwt, cancellationToken);

        // Parse claims from bearer token, and create CalimsPrincipal object.
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(jwt), "apiauth"));

        // Notify state changed.
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    /// <summary>
    /// Mark the user as logged out.
    /// </summary>
    public async ValueTask MarkUserAsLoggedOutAsync()
    {
        await RemoveTokenFromLocalStorageAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    /// <summary>
    /// Get logged in user's id.
    /// </summary>
    public async ValueTask<string> GetLoggedInUserIdAsync()
    {
        var jwt = await EnsureWithinExpirationAsync(
            await GetTokenFromLocalStorageAsync(),
            TimeSpan.FromDays(1));
        if (string.IsNullOrEmpty(jwt))
        {
            return string.Empty;
        }

        var userid = ParseClaimsFromJwt(jwt)
            .FirstOrDefault(claim => claim.Type == nameof(ResponseTwiHighUserContext.Id))?
            .Value ?? string.Empty;

        return userid;
    }

    public async ValueTask<string> GetLoggedInUserAvatarUrlAsync()
    {
        var jwt = await EnsureWithinExpirationAsync(
            await GetTokenFromLocalStorageAsync(),
            TimeSpan.FromDays(1));
        if (string.IsNullOrEmpty(jwt))
        {
            return string.Empty;
        }

        var url = ParseClaimsFromJwt(jwt)
            .FirstOrDefault(claim => claim.Type == nameof(ResponseTwiHighUserContext.AvatarUrl))?
            .Value ?? string.Empty;

        return url;
    }

    /// <summary>
    /// Parse claims from jwt.
    /// </summary>
    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var jwtSecurityToken = _defaultJwtSecurityTokenHandler.ReadJwtToken(jwt);
        return jwtSecurityToken.Payload.Claims;
    }

    /// <summary>
    /// Get the expiry datetime in UTC.
    /// </summary>
    private DateTime GetUtcExpiryFromJwt(string jwt)
    {
        if (string.IsNullOrEmpty(jwt))
        {
            return DateTime.MinValue;
        }

        var jwtSecurityToken = _defaultJwtSecurityTokenHandler.ReadJwtToken(jwt);
        if (jwtSecurityToken.Payload.Expiration.HasValue)
        {
            return DateTimeOffset.FromUnixTimeSeconds(jwtSecurityToken.Payload.Expiration.Value).UtcDateTime;
        }

        return DateTime.MinValue;
    }

    /// <summary>
    /// Ensure within expiration.
    /// </summary>
    private async ValueTask<string> EnsureWithinExpirationAsync(string jwt, TimeSpan expiration, CancellationToken cancellationToken = default)
        => DateTime.UtcNow.Add(expiration) < GetUtcExpiryFromJwt(jwt) ? jwt : await GetRefreshedTokenAsync(cancellationToken);

    /// <summary>
    /// Get a new bearer token from the server.
    /// </summary>
    private async ValueTask<string> GetRefreshedTokenAsync(CancellationToken cancellationToken = default)
    {
        var savedToken = await GetTokenFromLocalStorageAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(savedToken) || GetUtcExpiryFromJwt(savedToken) < DateTime.UtcNow)
        {
            // If don't has token, return string.Empty.
            return string.Empty;
        }

        // Set old Bearer token.
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);

        // Get new token from the server.
        var newToken = string.Empty;
        for (int i = 0; i < 5; i++)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var newTokenRes = await _httpClient.GetFromJsonAsync<ResponseJwtContext>(_apiUrlRefreshToken, cancellationToken).ConfigureAwait(false);
                newToken = newTokenRes?.Token ?? string.Empty;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                // If response status code is 503, wait a moment and retry.
                await Task.Delay(250 * (i + 1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // No action.
            }
        }

        // Return new Bearer token. If response is null, return string.Empty.
        return newToken;
    }

    /// <summary>
    /// Get a Bearer token from local storage in browser.
    /// </summary>
    public async ValueTask<string> GetTokenFromLocalStorageAsync(CancellationToken cancellationToken = default)
        => await _localStorage.GetItemAsStringAsync(LOCAL_STORAGE_NAME_JWT, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Set a Bearer token to local storage in browser.
    /// </summary>
    private async ValueTask SetTokenToLocalStorageAsync(string token, CancellationToken cancellationToken = default)
        => await _localStorage.SetItemAsStringAsync(LOCAL_STORAGE_NAME_JWT, token, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Remove a Bearer token from local storage in browser.
    /// </summary>
    private async ValueTask RemoveTokenFromLocalStorageAsync(CancellationToken cancellationToken = default)
        => await _localStorage.RemoveItemAsync(LOCAL_STORAGE_NAME_JWT, cancellationToken).ConfigureAwait(false);
}
