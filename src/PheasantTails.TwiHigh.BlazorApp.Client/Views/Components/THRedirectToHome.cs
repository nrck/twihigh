using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components;

public class THRedirectToHome : TwiHighComponentBase
{
    protected override void OnInitialized()
    {
        Navigation.NavigateToHomePage(replace: true);
    }
}
