using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Follows : TwiHighPageBase
{
    [Parameter]
    public string Id { get; set; } = string.Empty;

    [Inject]
    public IFollowsViewModel ViewModel { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SubscribeStateHasChanged(ViewModel.GetTwiHighUserFollowsCommand);
        SubscribeStateHasChanged(ViewModel.UserDisplayedOnScreen);
        SubscribeStateHasChanged(ViewModel.UserFollows);
        SubscribeStateHasChanged(ViewModel.PageTitle);
        SubscribeStateHasChanged(ViewModel.CanExequteGetTwiHighUserFollowsCommand);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await ViewModel.GetTwiHighUserFollowsCommand.ExecuteAsync(Id);
    }
}
