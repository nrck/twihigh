using Microsoft.AspNetCore.Authentication;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Views.Pages;

public partial class ServerLogout : TwiHighPageBase
{
    protected override async Task OnInitializedAsync()
    {
        await SignOutAsync().ConfigureAwait(false);
        Navigation.NavigateTo("/", forceLoad: true, replace: true);
    }

    private async Task SignOutAsync()
    {
        ArgumentNullException.ThrowIfNull(HttpContext, nameof(HttpContext));
        await HttpContext.SignOutAsync().ConfigureAwait(false);
    }
}
