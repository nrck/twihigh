using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.Feeds;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.Client.Services
{
    public class FeedService : IDisposable, IAsyncDisposable, IFeedService
    {
        private const string LOCAL_STORAGE_KEY_FEEDS = "UserFeeds_{0}";
        private readonly FeedHttpClient _feedHttpClient;
        private CancellationTokenSource _workerCancellationTokenSource;
        private AuthenticationStateProvider _authenticationStateProvider;
        private string _userId;

        public int FeedDotCount { get; set; }

        public ObservableCollection<FeedContext> FeedContexts { get; set; } = new ObservableCollection<FeedContext>();

        public Action NotifyChangedFeeds { get; set; }

        private string LocalStorageKeyFeeds => string.Format(LOCAL_STORAGE_KEY_FEEDS, _userId);

        private bool IsWorking { get; set; }

        public FeedService(FeedHttpClient feedHttpClient, AuthenticationStateProvider authenticationStateProvider)
        {
            _feedHttpClient = feedHttpClient;
            _workerCancellationTokenSource = new CancellationTokenSource();
            _authenticationStateProvider = authenticationStateProvider;
            _authenticationStateProvider.AuthenticationStateChanged += AuthenticationStateChangedHandlerAsync;
            _userId = string.Empty;
        }

        public async Task InitializeAsync(string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                return;
            }

            _feedHttpClient.SetToken(jwt);
            await SetUserIdAsync(_authenticationStateProvider.GetAuthenticationStateAsync());
            _ = GetMyFeedsWorkerAsync(_workerCancellationTokenSource.Token);
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

        private async Task SetUserIdAsync(Task<AuthenticationState> authenticationState)
        {
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
                var context = await _feedHttpClient.GetMyFeedsAsync(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
                if (context == null)
                {
                    continue;
                }
                MergeFeeds(context.Feeds);
                NotifyChangedFeeds.Invoke();
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
