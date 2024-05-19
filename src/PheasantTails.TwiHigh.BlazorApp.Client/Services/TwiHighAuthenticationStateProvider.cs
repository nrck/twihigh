using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using System.Security.Claims;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public class TwiHighAuthenticationStateProvider : AuthenticationStateProvider, IAuthenticationStateAccesser
{
    private readonly AuthenticationState _defaultAuthenticationState = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private Task<AuthenticationState> AuthenticationState { get; set; }

    public TwiHighAuthenticationStateProvider(PersistentComponentState state)
    {
        if (state.TryTakeFromJson(nameof(PersistentAuthenticationState), out PersistentAuthenticationState? persistentAuthenticationState) && persistentAuthenticationState != null)
        {
            Claim[] claims = [
                new(nameof(PersistentAuthenticationState.Id), persistentAuthenticationState.Id),
                new(nameof(PersistentAuthenticationState.DisplayId), persistentAuthenticationState.DisplayId),
                new(nameof(PersistentAuthenticationState.DisplayName), persistentAuthenticationState.DisplayName),
                new(nameof(PersistentAuthenticationState.AvatarUrl), persistentAuthenticationState.AvatarUrl),
                new(nameof(PersistentAuthenticationState.Token), persistentAuthenticationState.Token),
            ];
            AuthenticationState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"))));
        }
        else
        {
            AuthenticationState = Task.FromResult(_defaultAuthenticationState);
        }
    }

    /// <inheritdoc/>
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => AuthenticationState;

    /// <summary>
    /// Mark the user as logged out.
    /// </summary>
    public void MarkUserAsLoggedOut()
    {
        AuthenticationState = Task.FromResult(_defaultAuthenticationState);
        NotifyAuthenticationStateChanged(AuthenticationState);
    }

    /// <summary>
    /// Get logged in user's id.
    /// </summary>
    public ValueTask<string> GetLoggedInUserIdAsync()
        => FindFirstFromAuthenticationStateAsync(nameof(PersistentAuthenticationState.Id));

    /// <summary>
    /// Get logged in user's display id.
    /// </summary>
    public ValueTask<string> GetLoggedInUserDisplayIdAsync()
        => FindFirstFromAuthenticationStateAsync(nameof(PersistentAuthenticationState.DisplayId));

    /// <summary>
    /// Get logged in user's display name.
    /// </summary>
    public ValueTask<string> GetLoggedInUserDisplayNameAsync()
        => FindFirstFromAuthenticationStateAsync(nameof(PersistentAuthenticationState.DisplayName));

    /// <summary>
    /// Get logged in user's avatar url.
    /// </summary>
    public ValueTask<string> GetLoggedInUserAvatarUrlAsync()
        => FindFirstFromAuthenticationStateAsync(nameof(PersistentAuthenticationState.AvatarUrl));

    /// <summary>
    /// Get logged in user's api access token.
    /// </summary>
    public ValueTask<string> GetLoggedInUserTokenAsync()
        => FindFirstFromAuthenticationStateAsync(nameof(PersistentAuthenticationState.Token));

    private async ValueTask<string> FindFirstFromAuthenticationStateAsync(string type)
        => (await AuthenticationState.ConfigureAwait(false)).User.FindFirst(type)?.Value ?? string.Empty;
}
