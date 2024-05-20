using Microsoft.AspNetCore.Authentication;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Views.Pages;

/// <summary>
/// Logout page<br />
/// This page is static server side rendering(Static SSR).
/// </summary>
public partial class ServerLogout : TwiHighPageBase
{
    protected override async Task OnInitializedAsync()
    {
        await SignOutAsync().ConfigureAwait(false);

        // If Signout succed then navigate to index page.
        Navigation.NavigateTo("/", forceLoad: true, replace: true);
    }

    private async Task SignOutAsync()
    {
        // If HttpContext is null then throw exception. But on Static SSR, this exception is not throw because HttpContext is set by FW.
        ArgumentNullException.ThrowIfNull(HttpContext, nameof(HttpContext));

        // Delete Cookie.
        await HttpContext.SignOutAsync().ConfigureAwait(false);
    }
}
