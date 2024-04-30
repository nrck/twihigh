namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public interface IAuthenticationStateAccesser
{
    /// <summary>
    /// Get logged in user's avatar url.
    /// </summary>
    ValueTask<string> GetLoggedInUserAvatarUrlAsync();

    /// <summary>
    /// Get logged in user's display id.
    /// </summary>
    ValueTask<string> GetLoggedInUserDisplayIdAsync();

    /// <summary>
    /// Get logged in user's display name.
    /// </summary>
    ValueTask<string> GetLoggedInUserDisplayNameAsync();

    /// <summary>
    /// Get logged in user's id.
    /// </summary>
    ValueTask<string> GetLoggedInUserIdAsync();

    /// <summary>
    /// Get logged in user's api access token.
    /// </summary>
    ValueTask<string> GetLoggedInUserTokenAsync();
}
