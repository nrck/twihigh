using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Client.ViewModels;
using PheasantTails.TwiHigh.Data.Model;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Data.Store.Entity;
using System.Net;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Home : PageBase, IAsyncDisposable
    {
        /// <summary>
        /// ローカルストレージキー（タイムライン保存用）
        /// </summary>
        private const string LOCAL_STORAGE_KEY_TWEETS = "UserTimelines_{0}_v2";

        /// <summary>
        /// ローカルキャッシュするタイムラインのツイート数
        /// </summary>
        private const int LOCAL_CACHE_MAXIMUM_SIZE = 10000;

        private List<TweetViewModel> Tweets { get; set; } = new List<TweetViewModel>();

        private CancellationTokenSource? WorkerCancellationTokenSource { get; set; } = null;

        private string AvatarUrl { get; set; } = string.Empty;

        private Guid MyTwiHithUserId { get; set; }

        private bool IsProcessingMarkAsReaded { get; set; }

        public override async ValueTask DisposeAsync()
        {
            WorkerCancellationTokenSource?.Cancel();
            ScrollInfoService.OnScroll -= MarkAsReadedTweet;
            await ScrollInfoService.Disable();
            await base.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var id = (await AuthenticationState!).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value ?? string.Empty;
            if (Guid.TryParse(id, out var result))
            {
                MyTwiHithUserId = result;
            }
            await LoadTimelineFromLocalStorageAsync();
            WorkerCancellationTokenSource ??= new CancellationTokenSource();
            AvatarUrl = await GetMyAvatarUrlAsync();
            StateHasChanged();

            await ScrollInfoService.Enable();
            ScrollInfoService.OnScroll += MarkAsReadedTweet;

            await GetMyTimelineEvery5secAsync(WorkerCancellationTokenSource.Token);
        }

        private async Task GetMyTimelineEvery5secAsync(CancellationToken cancellationToken = default)
        {
            var since = DateTimeOffset.MinValue;
            if (Tweets != null && Tweets.Any())
            {
                since = Tweets.Max(tweet => tweet.UpdateAt);
            }
            var until = DateTimeOffset.MaxValue;
            while (!cancellationToken.IsCancellationRequested)
            {
                await GetTweetsAndMergeAsync(since, until);
                if (Tweets != null && Tweets.Any())
                {
                    since = Tweets.Max(tweet => tweet.UpdateAt);
                }
                else
                {
                    since = DateTimeOffset.MinValue;
                }
                await Task.Delay(5000, cancellationToken);
            }
        }

        private async Task<string> GetMyAvatarUrlAsync()
        {
            if (AuthenticationState is null)
            {
                await Task.Delay(5000);
                await GetMyAvatarUrlAsync();
                return string.Empty;
            }
            return (await AuthenticationState).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.AvatarUrl))?.Value ?? string.Empty;
        }

        private async Task LoadTimelineFromLocalStorageAsync()
        {
            var id = MyTwiHithUserId.ToString();
            var key = string.Format(LOCAL_STORAGE_KEY_TWEETS, id);
            Tweets = await LocalStorageService.GetItemAsync<List<TweetViewModel>>(key);
        }

        private async Task SaveTimelineToLocalStorageAsync()
        {
            var id = (await AuthenticationState!).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value ?? string.Empty;
            var key = string.Format(LOCAL_STORAGE_KEY_TWEETS, id);
            await LocalStorageService.SetItemAsync(key, Tweets.Take(LOCAL_CACHE_MAXIMUM_SIZE).ToArray());
        }

        private void MergeTimeline(List<TweetViewModel> source)
        {
            if (Tweets == null)
            {
                Tweets = source;
                return;
            }

            Tweets = source.UnionBy(Tweets, keySelector: tweet => tweet.Id)
                .OrderByDescending(tweet => tweet.CreateAt)
                .ToList();
            Tweets = Tweets.Where(tweet => tweet.ReplyTo.HasValue && string.IsNullOrEmpty(tweet.ReplyToUserDisplayId))
                .Select(tweet =>
                {
                    tweet.ReplyToUserDisplayId = Tweets.FirstOrDefault(t => t.Id == tweet.ReplyTo!.Value)?.UserDisplayId ?? string.Empty;
                    return tweet;
                })
                .UnionBy(Tweets, keySelector: tweet => tweet.Id)
                .OrderByDescending(tweet => tweet.CreateAt)
                .ToList();
        }

        private async Task GetTweetsAndMergeAsync(DateTimeOffset since, DateTimeOffset until)
        {
            ResponseTimelineContext? response = null;

            try
            {
                response = await TimelineHttpClient.GetMyTimelineAsync(since, until);
            }
            catch (HttpRequestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOutAsync();
                        Navigation.NavigateTo(DefinePaths.PAGE_PATH_LOGIN);
                        break;
                    default:
                        break;
                }
            }

            if (response != null && response.Tweets.Any())
            {
                MergeTimeline(response.Tweets.Select(t => new TweetViewModel(t)).ToList());
                if (response.Tweets.Length == 50)
                {
                    var systemTweet = TweetViewModel.SystemTweet;
                    systemTweet.Id = Guid.NewGuid();
                    systemTweet.Since = DateTimeOffset.MinValue;
                    systemTweet.Until = response.Oldest.AddTicks(-1);
                    systemTweet.CreateAt = response.Oldest.AddTicks(-1);
                    var tmp = new List<TweetViewModel> { systemTweet };
                    MergeTimeline(tmp);
                }
                await SaveTimelineToLocalStorageAsync();
                StateHasChanged();
            }
        }

        private async Task OnClickGetGapTweets(TweetViewModel tweet)
        {
            await GetTweetsAndMergeAsync(tweet.Since, tweet.Until);
            Tweets.Remove(tweet);
            await SaveTimelineToLocalStorageAsync();
            StateHasChanged();
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

        private void OnClickProfileEditor() => Navigation.NavigateTo(DefinePaths.PAGE_PATH_PROFILE_EDITOR);

        private void OnClickProfile(TweetViewModel tweetViewModel) => Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_PROFILE, tweetViewModel.UserDisplayId));

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
                    MergeTimeline(new List<TweetViewModel> { viewModel });
                }
            }
            else
            {
                SetErrorMessage("ツイートできませんでした。");
            }
        }

        private void OnClickDetail(TweetViewModel tweetViewModel) => Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_STATUS, tweetViewModel.UserDisplayId, tweetViewModel.Id));

        private async void MarkAsReadedTweet(object? sender, string[] ids)
        {
            if (IsProcessingMarkAsReaded)
            {
                return;
            }
            IsProcessingMarkAsReaded = true;
            List<Guid> tweetIds = new();
            foreach (var id in ids)
            {
                if (Guid.TryParse(id[6..], out var guid))
                {
                    tweetIds.Add(guid);
                }
            }

            Tweets.Where(tweet => tweetIds.Any(i => i == tweet.Id)).ToList().ForEach(tweet => tweet.IsReaded = true);
            await SaveTimelineToLocalStorageAsync();
            StateHasChanged();
            IsProcessingMarkAsReaded = false;
        }
    }
}
