using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.ViewModels;
using PheasantTails.TwiHigh.Data.Model;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Data.Store.Entity;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Status : PageBase
    {
        [Parameter]
        public string UserDisplayId { get; set; }

        [Parameter]
        public string TweetId { get; set; }

        private List<TweetViewModel> Tweets { get; set; } = new List<TweetViewModel>();

        private string Title { get; set; } = "ツイートを読込中";

        private Guid MyTwiHithUserId { get; set; }

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

            var tweets = await res.Content.ReadFromJsonAsync<List<Tweet>>();
            var main = tweets!.FirstOrDefault(t => t.Id == tweetId);
            if(main == null)
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
            Tweets = tweets!.Select(t => new TweetViewModel(t)).OrderBy(t=>t.CreateAt).ToList();
        }

        private async void OnClickDeleteButtonAsync(TweetViewModel model)
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
                var tweet = await res.Content.ReadFromJsonAsync<Tweet>();
                if (tweet != null)
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

        private void OnClickDetail(TweetViewModel tweetViewModel) => Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_STATUS, tweetViewModel.UserDisplayId, tweetViewModel.Id));
    }
}
