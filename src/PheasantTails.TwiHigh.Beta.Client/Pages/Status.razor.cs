using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using PheasantTails.TwiHigh.Beta.Client.ViewModels;
using PheasantTails.TwiHigh.Beta.Client.Extensions;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Interface;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Beta.Client.Pages
{
    public partial class Status : PageBase
    {
        [Parameter]
        public string UserDisplayId { get; set; }

        [Parameter]
        public string TweetId { get; set; }

        private List<TweetViewModel>? Tweets { get; set; }

        private string Title { get; set; } = "ツイートを読込中";

        private Guid MyTwiHithUserId { get; set; }

        private bool IsScrolling { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var id = (await AuthenticationState!).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value ?? string.Empty;
            if (Guid.TryParse(id, out var result))
            {
                MyTwiHithUserId = result;
            }
            Navigation.LocationChanged += OnLocationChanged;
        }

        protected override async Task OnParametersSetAsync()
        {
            // ツイートページ内で他のツイートが選択されたときのための初期化
            Title = "ツイートを読込中";
            Tweets = null;

            await base.OnParametersSetAsync();
            if (!Guid.TryParse(TweetId, out var tweetId))
            {
                SetWarnMessage("URLが間違っています。");
                Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME, replace: true);
                return;
            }

            var res = await TweetHttpClient.GetTweetAsync(tweetId);
            if (res == null)
            {
                SetErrorMessage("ツイートの取得に失敗しました。");
                Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME, replace: true);
                return;
            }

            if (!res.IsSuccessStatusCode)
            {
                SetWarnMessage("指定されたツイートを取得できませんでした。");
                Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME, replace: true);
                return;
            }

            var tweets = await res.Content.TwiHighReadFromJsonAsync<List<ITweet>>();
            var main = tweets!.FirstOrDefault(t => t.Id == tweetId);
            if (main == null)
            {
                SetWarnMessage("指定されたツイートは削除されています。");
                Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME, replace: true);
                return;
            }

            if (main.UserDisplayId != UserDisplayId)
            {
                Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_STATUS, main.UserDisplayId, TweetId), replace: true);
                return;
            }

            Title = $"{main.UserDisplayName}さんのツイート：{main.Text}";
            Tweets = tweets!.Select(t =>
            {
                var tmp = new TweetViewModel(t)
                {
                    IsReaded = true
                };
                if (tmp.Id == tweetId)
                {
                    tmp.IsEmphasized = true;
                    tmp.IsOpendReplyPostForm = Navigation.Uri.ToLower().EndsWith("reply");
                }
                return tmp;
            }).OrderBy(t => t.CreateAt).ToList();

            if (Navigation.Uri.ToLower().EndsWith("reply"))
            {
                var url = string.Format(DefinePaths.PAGE_PATH_STATUS, UserDisplayId, TweetId);
                Navigation.NavigateTo(url, false, true);
            }
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return Task.Run(async () =>
            {
                await ScrollAsync();
                await base.OnAfterRenderAsync(firstRender);
            });
        }

        protected override void Dispose(bool disposing)
        {
            Navigation.LocationChanged -= OnLocationChanged;
            base.Dispose(disposing);
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
                if (tweet != null && Tweets != null)
                {
                    var viewModel = new TweetViewModel(tweet);
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

        private void OnClickDetail(TweetViewModel tweetViewModel)
        {
            if (tweetViewModel.UserDisplayId == UserDisplayId &&
               tweetViewModel.Id.ToString() == TweetId)
            {
                // 同じページへの遷移はしない。
                return;
            }

            Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_STATUS, tweetViewModel.UserDisplayId, tweetViewModel.Id));
        }

        private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            await ScrollAsync();
        }

        private async Task ScrollAsync()
        {
            if (IsScrolling)
            {
                return;
            }
            IsScrolling = true;
            await Task.Delay(300);
            await ScrollToTweet(TweetId);
            IsScrolling = false;
        }
    }
}
