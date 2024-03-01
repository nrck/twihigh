using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Signup : TwiHighPageBase
{
    /// <summary>
    /// Gets or sets the SignupViewModel.
    /// </summary>
    [Inject]
    public ISignupViewModel ViewModel { get; set; } = default!;

    /// <summary>
    /// Initializes the component.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        SubscribeStateHasChanged(ViewModel.CanSignupCommand);
    }
}
