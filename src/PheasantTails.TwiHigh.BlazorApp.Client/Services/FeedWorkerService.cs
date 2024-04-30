using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Exceptions;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.Data.Model.Feeds;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;
using System.Web;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public class FeedWorkerService : IFeedWorkerService
{
    public const string LOCAL_STORAGE_KEY_USER_FEEDS_TIMELINE = "UserFeedsTimelines_{0}_v1";

    private readonly string _apiUrlBase;
    private readonly string _apiUrlGetMyFeeds;
    private readonly string _apiUrlPutOpenedMyFeeds;

    private LocalFeedsStore _store;
    private readonly ILocalStorageService _localStorageService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly HttpClient _httpClient;
    private bool _isDispose;
    private bool _isRunning;
    private CancellationTokenSource _cancellationTokenSource;

    public ReadOnlyCollection<DisplayFeed> FeedTimeline => _store?.FeedTimeline?.AsReadOnly() ?? ReadOnlyCollection<DisplayFeed>.Empty;

    public event Action? OnChangedFeedTimeline;

    private CancellationToken WorkerCancellationToken => _cancellationTokenSource.Token;

    public FeedWorkerService(
        ILocalStorageService localStorageService,
        AuthenticationStateProvider authenticationStateProvider,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _store = new LocalFeedsStore();
        _localStorageService = localStorageService;
        _authenticationStateProvider = authenticationStateProvider;
        _cancellationTokenSource = new CancellationTokenSource();
        _httpClient = httpClientFactory.CreateClient();

        // Set url
        _apiUrlBase = $"{configuration["FeedApiUrl"]}";
        _apiUrlGetMyFeeds = $"{_apiUrlBase}/?since={{0}}&until={{1}}";
        _apiUrlPutOpenedMyFeeds = $"{_apiUrlBase}/";

        _authenticationStateProvider.AuthenticationStateChanged += OnChangedAuthenticationState;
    }

    #region public
    public async ValueTask DisposeAsync()
    {
        if (_isDispose)
        {
            return;
        }
        _isDispose = true;
        await StopAsync();
        while (_isRunning)
        {
            await Task.Delay(1000);
        }
        _authenticationStateProvider.AuthenticationStateChanged -= OnChangedAuthenticationState;
        GC.SuppressFinalize(this);
    }

    public async ValueTask CacheClearAsync()
    {
        Guid userid = _store.UserId;
        _store = new LocalFeedsStore
        {
            UserId = userid
        };
        await ForceSaveAsync();
    }

    public async ValueTask ForceSaveAsync(CancellationToken cancellationToken = default)
        => await _localStorageService.SetItemAsync(GetLocalStorageKeyUserTimeline(), _store.GetSaveData(), cancellationToken);

    public async ValueTask ForceLoadAsync(CancellationToken cancellationToken = default)
        => _store = await _localStorageService.GetItemAsync<LocalFeedsStore>(GetLocalStorageKeyUserTimeline(), cancellationToken) ?? _store;

    public async ValueTask ForceFetchMyFeedTimelineAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken cancellationToken = default)
    {
        DisplayFeed[] feeds = await FetchMyFeedTimelineAsync(since, until, cancellationToken).ConfigureAwait(false);
        AddRange(feeds);
        OnChangedFeedTimeline?.Invoke();
    }

    public async Task MarkAsReadedFeedsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        Guid[] feedIds = ids.ToArray();
        if (feedIds.Length == 0)
        {
            return;
        }

        // Over 100 items, split to 100 items and request.
        for (int i = 0; i < feedIds.Length; i += 100)
        {
            PutUpdateMyFeedsContext context = new()
            {
                Ids = feedIds.Skip(i).Take(100).ToArray()
            };
            await EnsureSetAuthenticationHeaderValue().ConfigureAwait(false);
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(_apiUrlPutOpenedMyFeeds, context, cancellationToken: cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            OnChangedFeedTimeline?.Invoke();
        }
    }

    public async void Run() => await RunAsync().ConfigureAwait(false);

    public async void Stop() => await StopAsync().ConfigureAwait(false);
    #endregion

    #region private
    private int AddRange(IEnumerable<DisplayFeed> feeds)
    {
        foreach (DisplayFeed feed in feeds)
        {
            Upsert(feed);
        }
        TimelineOrderByDescending();

        return _store.FeedTimeline.Count;
    }

    private void Upsert(DisplayFeed feed)
    {
        int oldFeedIndex = _store.FeedTimeline.FindIndex(f => f.Id == feed.Id && f.UpdateAt < feed.UpdateAt);
        if (0 <= oldFeedIndex)
        {
            _store.FeedTimeline[oldFeedIndex] = feed;
        }
        else
        {
            _store.FeedTimeline.Add(feed);
        }
    }

    private void TimelineOrderByDescending()
        => _store.FeedTimeline = [.. _store.FeedTimeline.OrderByDescending(x => x.CreateAt)];

    private string GetLocalStorageKeyUserTimeline()
    {
        if (_store.UserId == default)
        {
            throw new TwiHighException("ローカルストレージにユーザーIDが見つかりませんでした。");
        }

        return string.Format(LOCAL_STORAGE_KEY_USER_FEEDS_TIMELINE, _store.UserId);
    }

    private async ValueTask<DisplayFeed[]> FetchMyFeedTimelineAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken cancellationToken = default)
    {
        if (until < since)
        {
            throw new ArgumentException("開始時刻と終了時刻が逆転しています。", nameof(since));
        }
        string url = string.Format(
            _apiUrlGetMyFeeds,
            HttpUtility.UrlEncode(since.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
            HttpUtility.UrlEncode(until.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"))
        );
        await EnsureSetAuthenticationHeaderValue().ConfigureAwait(false);
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return [];
        }
        ResponseFeedsContext context = await response.Content.TwiHighReadFromJsonAsync<ResponseFeedsContext>(cancellationToken: cancellationToken)
            ?? throw new TwiHighApiRequestException("通知の取得でエラーが発生しました。");
        DisplayFeed[] feeds = DisplayFeed.ConvertFrom(context.Feeds);
        if (feeds.Length == 1000)
        {
            // TODO: add sysytem item.
        }

        return feeds;
    }

    private async ValueTask RunAsync()
    {
        try
        {
            // If this worker is runnning, Do not processing.
            if (_isRunning)
            {
                return;
            }

            // Toggle the flag.
            _isRunning = true;

            // Check user id.
            string userId = await ((IAuthenticationStateAccesser)_authenticationStateProvider).GetLoggedInUserIdAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException($"未ログインで{nameof(TimelineWorkerService)}.{nameof(RunAsync)}()を実行することはできません。");
            }
            _store.UserId = Guid.Parse(userId);

            // Load timeline data from local storage.
            if (!await _localStorageService.ContainKeyAsync(GetLocalStorageKeyUserTimeline(), WorkerCancellationToken).ConfigureAwait(false))
            {
                await ForceSaveAsync(WorkerCancellationToken).ConfigureAwait(false);
            }
            await ForceLoadAsync(WorkerCancellationToken).ConfigureAwait(false);
            if (0 < FeedTimeline.Count)
            {
                OnChangedFeedTimeline?.Invoke();
            }

            // If cancellation requested or this instance disposed, break this loop.
            while (!WorkerCancellationToken.IsCancellationRequested && !_isDispose)
            {
                // Do REST API.
                DisplayFeed[] feeds = await FetchMyFeedTimelineAsync(_store.Latest.AddTicks(1), DateTimeOffset.MaxValue, WorkerCancellationToken).ConfigureAwait(false);

                if (0 < feeds.Length)
                {
                    AddRange(feeds);
                    OnChangedFeedTimeline?.Invoke();

                    // Save timeline data to local storage.
                    await ForceSaveAsync(WorkerCancellationToken);
                }

                // Interval.
                await Task.Delay(10000, WorkerCancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            // Toggle the flag.
            _isRunning = false;
        }
    }

    private async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            return;
        }
        await _cancellationTokenSource.CancelAsync();

        while (_isRunning)
        {
            await Task.Delay(100, cancellationToken);
        }
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private async void OnChangedAuthenticationState(Task<AuthenticationState> authenticationState)
        => await SetAuthenticationHeaderValue(authenticationState);

    private async Task SetAuthenticationHeaderValue(Task<AuthenticationState> authenticationState)
    {
        AuthenticationState state = await authenticationState;
        if (!(state?.User?.Identity?.IsAuthenticated ?? false))
        {
            // Not Authenticated.
            if (_store.UserId != default)
            {
                await ForceSaveAsync();
                _store = new();
            }
            await StopAsync();

            return;
        }

        // Set new user id.
        string? userIdFromClaims = state.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
        if (Guid.TryParse(userIdFromClaims, out Guid userId) && _store.UserId != userId)
        {
            // If logged in user was changed, Load new user's timeline to local timeline store. 
            if (_store.UserId != default)
            {
                await ForceSaveAsync();
            }
            _store = new()
            {
                UserId = userId
            };
            await ForceLoadAsync();
        }

        // Set new bearer token.
        string token = await ((IAuthenticationStateAccesser)_authenticationStateProvider).GetLoggedInUserTokenAsync().ConfigureAwait(false);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task EnsureSetAuthenticationHeaderValue()
    {
        if (_httpClient.DefaultRequestHeaders.Authorization == null)
        {
            await SetAuthenticationHeaderValue(_authenticationStateProvider.GetAuthenticationStateAsync()).ConfigureAwait(false);
        }
    }
    #endregion
}
