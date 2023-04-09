﻿using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Data.Store.Entity;
using System.Net;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Home : PageBase, IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// ローカルストレージキー（タイムライン保存用）
        /// </summary>
        private const string LOCAL_STORAGE_KEY_TWEETS = "UserTimelines_{0}";

        /// <summary>
        /// ローカルキャッシュするタイムラインのツイート数
        /// </summary>
        private const int LOCAL_CACHE_MAXIMUM_SIZE = 1000;

        private Tweet[] Tweets { get; set; } = Array.Empty<Tweet>();

        private CancellationTokenSource? WorkerCancellationTokenSource { get; set; } = null;

        private string AvatarUrl { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadTimelineFromLocalStorageAsync();
            WorkerCancellationTokenSource ??= new CancellationTokenSource();
            AvatarUrl = await GetMyAvatarUrlAsync();
            StateHasChanged();

            await GetMyTimerlineEvery5secAsync(WorkerCancellationTokenSource.Token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorkerCancellationTokenSource?.Cancel();
                base.Dispose(disposing);
            }
        }

        private async Task GetMyTimerlineEvery5secAsync(CancellationToken cancellationToken = default)
        {
            ResponseTimelineContext? response = null;
            var since = DateTimeOffset.MinValue;
            var until = DateTimeOffset.MaxValue;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (Tweets != null && Tweets.Any())
                    {
                        since = Tweets.Max(tweet => tweet.CreateAt);
                    }
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
                    MergeTimeline(response.Tweets);
                    await SaveTimelineToLocalStorageAsync();
                    StateHasChanged();
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
            var id = (await AuthenticationState!).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value ?? string.Empty;
            var key = string.Format(LOCAL_STORAGE_KEY_TWEETS, id);
            Tweets = await LocalStorageService.GetItemAsync<Tweet[]>(key);
        }

        private async Task SaveTimelineToLocalStorageAsync()
        {
            var id = (await AuthenticationState!).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value ?? string.Empty;
            var key = string.Format(LOCAL_STORAGE_KEY_TWEETS, id);
            await LocalStorageService.SetItemAsync(key, Tweets);
        }

        private void MergeTimeline(Tweet[] source)
        {
            if (Tweets == null)
            {
                Tweets = source;
                return;
            }

            Tweets = Tweets.UnionBy(source, keySelector: tweet => tweet.Id)
                .OrderByDescending(tweet => tweet.CreateAt)
                .Take(LOCAL_CACHE_MAXIMUM_SIZE)
                .ToArray();
        }
    }
}
