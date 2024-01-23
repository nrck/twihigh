using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using System.Net;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages
{
    public partial class Home : TwiHighPageBase, IAsyncDisposable
    {
        /// <summary>
        /// ローカルキャッシュするタイムラインのツイート数
        /// </summary>
        private const int LOCAL_CACHE_MAXIMUM_SIZE = 10000;

        private List<TweetViewModel>? Tweets { get; set; }

        private CancellationTokenSource? WorkerCancellationTokenSource { get; set; } = null;

        private string AvatarUrl { get; set; } = string.Empty;

        private Guid MyTwiHithUserId { get; set; }

        private bool IsProcessingMarkAsReaded { get; set; }





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
            if (Tweets == null)
            {
                return;
            }
            var id = (await AuthenticationState!).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value ?? string.Empty;
            var key = string.Format(LOCAL_STORAGE_KEY_TWEETS, id);
            await LocalStorageService.SetItemAsync(key, Tweets.Take(LOCAL_CACHE_MAXIMUM_SIZE).ToArray());
        }


        private async void MarkAsReadedTweet(object? sender, string[] ids)
        {
            if (IsProcessingMarkAsReaded || Tweets == null)
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
