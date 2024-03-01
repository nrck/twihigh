using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Login : TwiHighPageBase
{
    [Inject]
    public ILoginViewModel ViewModel { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SubscribeStateHasChanged(ViewModel.CheckAuthenticationStateCommand);
        if (IsServerSideRendering == false)
        {
            await ViewModel.CheckAuthenticationStateCommand.ExecuteAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnInitializedAsync();
        if (firstRender && IsServerSideRendering)
        {
            await ViewModel.CheckAuthenticationStateCommand.ExecuteAsync();
        }
    }
}
