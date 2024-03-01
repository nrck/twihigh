using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Followers : TwiHighPageBase
{
    [Parameter]
    public string Id { get; set; } = string.Empty;

    [Inject]
    public IFollowersViewModel ViewModel { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SubscribeStateHasChanged(ViewModel.GetTwiHighUserFollowersCommand);
        SubscribeStateHasChanged(ViewModel.UserDisplayedOnScreen);
        SubscribeStateHasChanged(ViewModel.UserFollowers);
        SubscribeStateHasChanged(ViewModel.PageTitle);
        SubscribeStateHasChanged(ViewModel.CanExequteGetTwiHighUserFollowersCommand);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await ViewModel.GetTwiHighUserFollowersCommand.ExecuteAsync(Id);
    }
}
