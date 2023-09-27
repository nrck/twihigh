using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Client.ViewModels;
using PheasantTails.TwiHigh.Data.Model;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using System.Net;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Profile : PageBase
    {
        [Parameter]
        public string? Id { get; set; }

        private ResponseTwiHighUserContext? User { get; set; }

        private bool IsFollowing { get; set; }

        private bool IsFollowed { get; set; }

        private bool IsMyTwiHighUser { get; set; }

        private string Title { get; set; } = "プロフィール読み込み中";

        private List<TweetViewModel>? Tweets { get; set; }

        private Guid MyTwiHithUserId { get; set; }

        private bool IsProcessing { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var id = (await AuthenticationState!).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value ?? string.Empty;
            if (Guid.TryParse(id, out var result))
            {
                MyTwiHithUserId = result;
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            User = null;
            Tweets = null;
            IsMyTwiHighUser = false;

            // ID指定なしでログインユーザーのプロフィールページへ遷移
            if (string.IsNullOrEmpty(Id))
            {
                Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_PROFILE, MyTwiHithUserId), false, true);
                return;
            }

            User = await AppUserHttpClient.GetTwiHighUserAsync(Id);
            if (User == null)
            {
                Title = "プロフィールを読み込めませんでした。";
            }
            else if (User.DisplayId != Id)
            {
                // GuidからDisplayIdのページへ遷移
                Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_PROFILE, User.DisplayId), false, true);
                return;
            }
            else
            {
                Title = $"{User.DisplayName}（@{User.DisplayId}）";
            }
            if (User != null)
            {
                var res = await TweetHttpClient.GetUserTweetsAsync(User.Id);
                if (res != null && res.IsSuccessStatusCode && res.StatusCode == HttpStatusCode.OK)
                {
                    var tweets = await res.Content.ReadFromJsonAsync<TweetViewModel[]>();
                    Tweets = tweets!.Select(t => new TweetViewModel(t) { IsReaded = true })
                        .OrderByDescending(t => t.CreateAt)
                        .ToList();
                }
                else
                {
                    Tweets = new List<TweetViewModel>();
                }
            }
            await SetFollowButtonAsync();
            await base.OnParametersSetAsync();
        }

        private async Task OnClickFollowButton()
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
                var res = await FollowHttpClient.PutNewFolloweeAsync(User.Id.ToString());
                res.EnsureSuccessStatusCode();
                SetSucessMessage($"@{User.DisplayId}さんをフォローしました！");
                User = await AppUserHttpClient.GetTwiHighUserAsync(Id);
            }
            catch (HttpRequestException ex)
            {
                SetErrorMessage("フォローできませんでした。");
            }
            finally
            {
                IsProcessing = false;
            }
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

        private async Task OnClickDeleteButtonAsync(TweetViewModel model)
        {
            await DeleteMyTweet(model.Id);
        }

        private async Task DeleteMyTweet(Guid tweetId)
        {
            var res = await TweetHttpClient.DeleteTweetAsync(tweetId);
            if (res != null && res.IsSuccessStatusCode)
            {
                SetSucessMessage("ツイートを削除しました！");
            }
            else
            {
                SetErrorMessage("ツイートを削除できませんでした。");
            }
        }

        private async Task PostTweetAsync(PostTweetContext postTweet)
        {
            var res = await TweetHttpClient.PostTweetAsync(postTweet);
            if (res != null && res.IsSuccessStatusCode)
            {
                SetSucessMessage("ツイートを送信しました！");
                var tweet = await res.Content.ReadFromJsonAsync<TweetViewModel>();
                if (tweet != null)
                {
                    var viewModel = tweet;
                    Tweets.Add(viewModel);
                    Tweets = Tweets.OrderBy(t => t.CreateAt).ToList();
                }
            }
            else
            {
                SetErrorMessage("ツイートできませんでした。");
            }
        }

        private void OnClickProfileEditor() => Navigation.NavigateTo(DefinePaths.PAGE_PATH_PROFILE_EDITOR);

        private void OnClickProfile(TweetViewModel tweetViewModel) => Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_PROFILE, tweetViewModel.UserDisplayId));

        private void OnClickDetail(TweetViewModel tweetViewModel) => Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_STATUS, tweetViewModel.UserDisplayId, tweetViewModel.Id));
    }
}
