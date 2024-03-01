using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Profile : TwiHighPageBase
{
    [Inject]
    public IProfileViewModel ViewModel { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }

    private bool IsFollowing { get; set; }

    private bool IsFollowed { get; set; }

    private Guid MyTwiHithUserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        string id = await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).GetLoggedInUserIdAsync().ConfigureAwait(false);
        if (Guid.TryParse(id, out Guid result))
        {
            MyTwiHithUserId = result;
        }
        SubscribeStateHasChanged(ViewModel.GetUserTimelineCommand);
        SubscribeStateHasChanged(ViewModel.FollowUserDisplayedOnScreenCommand);
        SubscribeStateHasChanged(ViewModel.RemoveUserDisplayedOnScreenCommand);
        SubscribeStateHasChanged(ViewModel.UserDisplayedOnScreen);
        SubscribeStateHasChanged(ViewModel.PageTitle);
        SubscribeStateHasChanged(ViewModel.DeleteMyTweetCommand);
        SubscribeStateHasChanged(ViewModel.PostTweetCommand);
        SubscribeStateHasChanged(ViewModel.CanExecuteFollowOrRemove);
        SubscribeStateHasChanged(ViewModel.IsMyTwiHighUser);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        Console.WriteLine($"ID is {Id}!!");
        await ViewModel.GetUserTimelineCommand.ExecuteAsync(Id);
        await SetFollowButtonAsync();
    }

    private async Task OnClickFollowButton()
    {
        await ViewModel.FollowUserDisplayedOnScreenCommand.ExecuteAsync();
        await SetFollowButtonAsync();
    }

    private async Task OnClickRemoveButton()
    {
        await ViewModel.RemoveUserDisplayedOnScreenCommand.ExecuteAsync();
        await SetFollowButtonAsync();
    }

    private async Task SetFollowButtonAsync()
    {
        if (ViewModel.UserDisplayedOnScreen.Value == null || AuthenticationState == null)
        {
            return;
        }
        AuthenticationState state = await AuthenticationState;
        if (state == null || state.User.Identity == null || !state.User.Identity.IsAuthenticated)
        {
            return;
        }
        string? id = state.User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value;
        if (Guid.TryParse(id, out Guid myGuid))
        {
            IsFollowing = ViewModel.UserDisplayedOnScreen.Value.Followers.Any(guid => guid == myGuid);
            IsFollowed = ViewModel.UserDisplayedOnScreen.Value.Follows.Any(guid => guid == myGuid);
            ViewModel.IsMyTwiHighUser.Value = ViewModel.UserDisplayedOnScreen.Value.Id == myGuid;
        }
        StateHasChanged();
    }
}
