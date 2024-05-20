using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using System.Security.Claims;

namespace PheasantTails.TwiHigh.BlazorApp.Services;

/// <summary>
/// AuthenticationStateProvider for server side.
/// </summary>
internal sealed class TwiHighServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IAuthenticationStateAccesser, IDisposable
{
    /// <summary>
    /// PersistentComponentState for persistent to WebAssembly(Client side).
    /// </summary>
    private readonly PersistentComponentState _state;

    /// <summary>
    /// Persistent subscription.
    /// </summary>
    private readonly PersistingComponentStateSubscription _subscription;

    /// <summary>
    /// Server side AuthenticationState.
    /// </summary>
    private Task<AuthenticationState>? _authenticationStateTask;

    public TwiHighServerAuthenticationStateProvider(PersistentComponentState persistentComponentState)
    {
        _state = persistentComponentState;

        // Register authentication state changed handler.
        AuthenticationStateChanged += OnAuthenticationStateChanged;

        // Create persisting subscription.
        _subscription = _state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    public void Dispose()
    {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
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

    /// <summary>
    /// Get Claims that match the argument from ClaimsPrincipal in authentication state task.<br />
    /// If not match the argument, this method return <see cref="string.Empty"/>.
    /// </summary>
    /// <param name="type">Claims type name</param>
    /// <returns>Claim value</returns>
    private async ValueTask<string> FindFirstFromAuthenticationStateAsync(string type)
    {
        if (_authenticationStateTask is null)
        {
            return string.Empty;
        }
        else
        {
            return (await _authenticationStateTask.ConfigureAwait(false)).User.FindFirst(type)?.Value ?? string.Empty;
        }
    }

    /// <summary>
    /// Authentication state changed handler.
    /// </summary>
    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        => _authenticationStateTask = task;

    /// <summary>
    /// Persisting handler.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            throw new InvalidOperationException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        AuthenticationState authenticationState = await _authenticationStateTask;
        ClaimsPrincipal principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            string? id = principal.FindFirst(nameof(PersistentAuthenticationState.Id))?.Value;
            string? displayId = principal.FindFirst(nameof(PersistentAuthenticationState.DisplayId))?.Value;
            string? displayName = principal.FindFirst(nameof(PersistentAuthenticationState.DisplayName))?.Value;
            string? avatarUrl = principal.FindFirst(nameof(PersistentAuthenticationState.AvatarUrl))?.Value;
            string? token = principal.FindFirst(nameof(PersistentAuthenticationState.Token))?.Value;

            if (id != null && displayId != null && displayName != null && avatarUrl != null && token != null)
            {
                PersistentAuthenticationState state = new PersistentAuthenticationState(id, displayId, displayName, avatarUrl, token);
                // Persist for WebAssembly.
                _state.PersistAsJson(nameof(PersistentAuthenticationState), state);
            }
        }
    }
}
