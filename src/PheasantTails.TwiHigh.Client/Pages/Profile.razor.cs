using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Profile : PageBase
    {
        [Parameter]
        public string Id { get; set; } = string.Empty;

        private ResponseTwiHighUserContext? User { get; set; }

        private bool IsFollowing { get; set; }

        private bool IsFollowed { get; set; }

        private bool IsMyTwiHighUser { get; set; }

        private string Title { get; set; } = "プロフィール読み込み中";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            User = await AppUserHttpClient.GetTwiHighUserAsync(Id);
            if (User == null)
            {
                Title = "プロフィールを読み込めませんでした。";
            }
            else
            {
                Title = $"{User.DisplayName}（@{User.DisplayId}）";
            }
            StateHasChanged();
            await SetFollowButtonAsync();
        }

        private async Task OnClickFollowButton()
        {
            if (User == null)
            {
                return;
            }
            try
            {
                var res = await FollowHttpClient.PutNewFollowee(User.Id.ToString());
            }
            catch (HttpRequestException ex)
            {
            }

            try
            {
                User = await AppUserHttpClient.GetTwiHighUserAsync(Id);
            }
            catch (HttpRequestException ex)
            {
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
}
