using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class ProfileEditer : TwiHighPageBase
{
    [Inject]
    public IProfileEditerViewModel ViewModel { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SubscribeStateHasChanged(ViewModel.LoadLocalFileCommand);
        SubscribeStateHasChanged(ViewModel.SaveCommand);
        SubscribeStateHasChanged(ViewModel.AvatarResetCommand);
        SubscribeStateHasChanged(ViewModel.GetUserCommand);
        SubscribeStateHasChanged(ViewModel.CanExecuteSaveCommand);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await ViewModel.GetUserCommand.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
