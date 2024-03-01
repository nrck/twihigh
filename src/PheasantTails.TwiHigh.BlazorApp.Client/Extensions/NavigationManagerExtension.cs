using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Interface;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

internal static class NavigationManagerExtension
{
    internal const string PAGE_PATH_HOME = "/home";
    internal const string PAGE_PATH_INDEX = "/";
    internal const string PAGE_PATH_LOGIN = "/login";
    internal const string PAGE_PATH_LOGOUT = "/logout";
    internal const string PAGE_PATH_PROFILE = "/profile/{0}";
    internal const string PAGE_PATH_PROFILE_EDITOR = "/profile/editor";
    internal const string PAGE_PATH_SIGNUP = "/signup";
    internal const string PAGE_PATH_STATUS = "/{0}/status/{1}";
    internal const string PAGE_PATH_CLEAR_LOCALDATA = "/clear";
    internal const string PAGE_PATH_LICENCE = "/licence";
    internal const string PAGE_PATH_FEED = "/feeds";

    internal static void NavigateToStatePage(this NavigationManager navigationManager, ITweet tweet, bool isReply = false, bool forceLoad = false, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);
        ArgumentNullException.ThrowIfNull(tweet);
        if (tweet == null)
        {
            throw new ArgumentException($"'{nameof(tweet)}' を NULL または空にすることはできません。", nameof(tweet));
        }
        if (isReply)
        {
            navigationManager.NavigateTo($"{string.Format(PAGE_PATH_STATUS, tweet.UserDisplayId, tweet.Id)}/reply", forceLoad, replace);
        }
        else
        {
            navigationManager.NavigateTo(string.Format(PAGE_PATH_STATUS, tweet.UserDisplayId, tweet.Id), forceLoad, replace);
        }
    }

    internal static void NavigateToProfilePage(this NavigationManager navigationManager, ITwiHighUserSummary user, bool forceLoad = false, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);
        ArgumentNullException.ThrowIfNull(user);

        navigationManager.NavigateToProfilePage(user.UserDisplayId, forceLoad, replace);
    }

    internal static void NavigateToProfilePage(this NavigationManager navigationManager, string userDisplayIdOrGuidString, bool forceLoad = false, bool replace = false)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);
        if (string.IsNullOrEmpty(userDisplayIdOrGuidString))
        {
            throw new ArgumentException($"'{nameof(userDisplayIdOrGuidString)}' を NULL または空にすることはできません。", nameof(userDisplayIdOrGuidString));
        }

        navigationManager.NavigateTo(string.Format(PAGE_PATH_PROFILE, userDisplayIdOrGuidString), forceLoad, replace);
    }

    internal static void NavigateToHomePage(this NavigationManager navigationManager, bool forceLoad = false, bool replace = false)
        => navigationManager.NavigateTo(PAGE_PATH_HOME, forceLoad, replace);

    internal static void NavigateToLoginPage(this NavigationManager navigationManager, bool forceLoad = false, bool replace = false)
        => navigationManager.NavigateTo(PAGE_PATH_LOGIN, forceLoad, replace);

    internal static void NavigateToProfileEditorPage(this NavigationManager navigationManager, bool forceLoad = false, bool replace = false)
        => navigationManager.NavigateTo(PAGE_PATH_PROFILE_EDITOR, forceLoad, replace);
}
