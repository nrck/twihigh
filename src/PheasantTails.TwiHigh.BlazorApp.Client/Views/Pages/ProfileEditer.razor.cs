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
        await base.OnInitializedAsync().ConfigureAwait(false);
        SubscribeStateHasChanged(ViewModel.LoadLocalFileCommand);
        SubscribeStateHasChanged(ViewModel.SaveCommand);
        SubscribeStateHasChanged(ViewModel.AvatarResetCommand);
        SubscribeStateHasChanged(ViewModel.GetUserCommand);
        SubscribeStateHasChanged(ViewModel.CanExecuteSaveCommand);
        SubscribeStateHasChanged(ViewModel.CanExecuteSaveCommand);
        SubscribeStateHasChanged(ViewModel.User);
        SubscribeStateHasChanged(ViewModel.AvatarUrl);
        SubscribeStateHasChanged(ViewModel.DisplayName);
        SubscribeStateHasChanged(ViewModel.DisplayId);
        SubscribeStateHasChanged(ViewModel.Biography);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);
        if (firstRender)
        {
            await ViewModel.GetUserCommand.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
