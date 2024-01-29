using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Profile : TwiHighPageBase
{
    [Inject]
    public ProfileViewModel ViewModel { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }

    private bool IsFollowing { get; set; }

    private bool IsFollowed { get; set; }

    private Guid MyTwiHithUserId { get; set; }

    private bool IsProcessing { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        string id = await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).GetLoggedInUserIdAsync().ConfigureAwait(false);
        if (Guid.TryParse(id, out Guid result))
        {
            MyTwiHithUserId = result;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await ViewModel.GetUserTimelineCommand.ExecuteAsync(Id);
        await SetFollowButtonAsync();
    }

    private async Task OnClickFollowButton()
    {
        await ViewModel.FollowUserDisplayedOnScreenCommand.ExecuteAsync();
        await SetFollowButtonAsync();
        StateHasChanged();
    }

    private async Task OnClickRemoveButton()
    {
        if (User == null)
        {
            return;
        }
        if (IsProcessing)
        {
            return;
        }
        IsProcessing = true;
        try
        {
            var res = await FollowHttpClient.DeleteFolloweeAsync(User.Id.ToString());
            res.EnsureSuccessStatusCode();
            SetInfoMessage($"@{User.DisplayId}さんをリムーブしました！");
            User = await AppUserHttpClient.GetTwiHighUserAsync(Id);
        }
        catch (HttpRequestException ex)
        {
            SetErrorMessage("リムーブできませんでした。");
        }
        finally
        {
            IsProcessing = false;
        }
        await SetFollowButtonAsync();
        StateHasChanged();
    }

    private async Task SetFollowButtonAsync()
    {
        if (User == null || AuthenticationState == null)
        {
            return;
        }
        var state = await AuthenticationState;
        if (state == null || state.User.Identity == null || !state.User.Identity.IsAuthenticated)
        {
            return;
        }
        var id = state.User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value;
        if (Guid.TryParse(id, out var myGuid))
        {
            IsFollowing = User.Followers.Any(guid => guid == myGuid);
            IsFollowed = User.Follows.Any(guid => guid == myGuid);
            IsMyTwiHighUser = User.Id == myGuid;
        }
        StateHasChanged();
    }
}
