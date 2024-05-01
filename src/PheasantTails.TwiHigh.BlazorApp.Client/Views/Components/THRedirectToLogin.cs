using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components;

public class THRedirectToLogin : TwiHighComponentBase
{
    protected override void OnInitialized()
    {
        Navigation.NavigateToLoginPage(replace: true);
    }
}
