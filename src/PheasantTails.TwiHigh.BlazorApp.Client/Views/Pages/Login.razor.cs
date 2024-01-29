using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Login : TwiHighPageBase
{
    [Inject]
    public ILoginViewModel ViewModel { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SubscribeStateHasChanged(ViewModel.CheckAuthenticationStateCommand);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnInitializedAsync();
        await ViewModel.CheckAuthenticationStateCommand.ExecuteAsync();
    }
}
