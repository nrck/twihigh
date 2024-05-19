using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Logout : TwiHighPageBase
{
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            LoggedOut();
            Navigation.NavigateToServerLogoutPage(replace: true);
        }
    }

    private void LoggedOut()
    {
        if (AuthenticationStateProvider is TwiHighAuthenticationStateProvider twiHighAuthenticationStateProvider)
        {
            twiHighAuthenticationStateProvider.MarkUserAsLoggedOut();
        }
    }
}
