using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace PheasantTails.TwiHigh.BlazorApp.Services;

internal sealed class TwiHighServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IAuthenticationStateAccesser, IDisposable
{
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;
    private Task<AuthenticationState>? _authenticationStateTask;

    public TwiHighServerAuthenticationStateProvider(PersistentComponentState persistentComponentState)
    {
        _state = persistentComponentState;

        AuthenticationStateChanged += OnAuthenticationStateChanged;
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

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
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
                _state.PersistAsJson(nameof(PersistentAuthenticationState), state);
            }
        }
    }
}
