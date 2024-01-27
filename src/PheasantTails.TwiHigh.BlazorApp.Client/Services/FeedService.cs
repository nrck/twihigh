using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.Data.Model.Feeds;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services
{
    public class FeedService : IDisposable, IAsyncDisposable, IFeedService
    {
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _workerCancellationTokenSource;
        private readonly TwiHighAuthenticationStateProvider _authenticationStateProvider;

        public int FeedDotCount { get; set; }

        public ObservableCollection<FeedContext> FeedContexts { get; set; } = new ObservableCollection<FeedContext>();

        public event Action? NotifyChangedFeeds;

        private bool IsWorking { get; set; }

        public FeedService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _workerCancellationTokenSource = new CancellationTokenSource();
            _authenticationStateProvider = (TwiHighAuthenticationStateProvider)authenticationStateProvider;
            _authenticationStateProvider.AuthenticationStateChanged += AuthenticationStateChangedHandlerAsync;
        }

        public async Task InitializeAsync(string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                return;
            }

            _httpClient.SetToken(jwt);
            await SetUserIdAsync(_authenticationStateProvider.GetAuthenticationStateAsync());
            FeedContexts = new ObservableCollection<FeedContext>();
            _ = GetMyFeedsWorkerAsync(_workerCancellationTokenSource.Token);
        }

        public async Task MarkAsReadedFeedAsync(IEnumerable<Guid> ids)
        {
            var opendFeeds = FeedContexts.Where(f => f.IsOpened == false && ids.Any(i => i == f.Id)).ToList();
            opendFeeds.ForEach(f => f.IsOpened = true);
            MergeFeeds(opendFeeds);

            var context = new PutUpdateMyFeedsContext
            {
                Ids = opendFeeds.Select(f => f.Id).ToArray()
            };
            await _httpClient.PutOpenedMyFeeds(context);
        }

        public ValueTask DisposeAsync()
        {
            WorkerDispose();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            WorkerDispose();
            GC.SuppressFinalize(this);
        }

        private async void AuthenticationStateChangedHandlerAsync(Task<AuthenticationState> authenticationState)
        {
            await SetUserIdAsync(authenticationState);
        }

        private Task SetUserIdAsync(Task<AuthenticationState> authenticationState)
        {
            return Task.CompletedTask;
        }

        private async Task GetMyFeedsWorkerAsync(CancellationToken cancellationToken)
        {
            if (IsWorking)
            {
                return;
            }

            IsWorking = true;
            await Task.Delay(5000, cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await _httpClient.GetMyFeedsAsync(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
                if (context == null)
                {
                    continue;
                }
                MergeFeeds(context.Feeds);
                NotifyChangedFeeds?.Invoke();
                await Task.Delay(5000, cancellationToken);
            }
            IsWorking = false;
        }

        private void MergeFeeds(IEnumerable<FeedContext> newFeeds)
        {
            var tmp = newFeeds.UnionBy(FeedContexts, f => f.Id)
                .OrderByDescending(f => f.CreateAt)
                .ToArray();
            FeedContexts = new ObservableCollection<FeedContext>(tmp);
            FeedDotCount = FeedContexts.Count(f => !f.IsOpened);
        }

        private void WorkerDispose()
        {
            _workerCancellationTokenSource.Cancel();
            _workerCancellationTokenSource.Dispose();
            _authenticationStateProvider.AuthenticationStateChanged -= AuthenticationStateChangedHandlerAsync;
        }
    }
}
