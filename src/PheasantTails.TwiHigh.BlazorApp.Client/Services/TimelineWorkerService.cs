using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Exceptions;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public class TimelineWorkerService : ITimelineWorkerService
{
    public const string LOCAL_STORAGE_KEY_USER_TIMELINE = "UserTimelines_{0}_v3";

    private readonly string _apiUrlBase;
    private readonly string _apiUrlTweet;
    private readonly string _apiUrlDeleteTweet;
    private readonly string _apiUrlGetTweet;
    private readonly string _apiUrlGetUserTweets;
    private readonly string _apiUrlTimeline;

    private LocalTimelineStore _store;
    private readonly ILocalStorageService _localStorageService;
    private readonly TwiHighAuthenticationStateProvider _authenticationStateProvider;
    private readonly HttpClient _httpClient;
    private bool _isDispose;
    private bool _isRunning;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly IMessageService _messageService;

    public ReadOnlyCollection<DisplayTweet> Timeline => _store.Timeline.AsReadOnly<DisplayTweet>();

    public event Action? OnChangedTimeline;

    private CancellationToken WorkerCancellationToken => _cancellationTokenSource.Token;


    public TimelineWorkerService(
        ILocalStorageService localStorageService,
        AuthenticationStateProvider authenticationStateProvider,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IMessageService messageService)
    {
        _store = new LocalTimelineStore();
        _localStorageService = localStorageService;
        _authenticationStateProvider = authenticationStateProvider as TwiHighAuthenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
        _cancellationTokenSource = new CancellationTokenSource();
        _httpClient = httpClientFactory.CreateClient();
        _messageService = messageService;

        _apiUrlBase = $"{configuration["TweetApiUrl"]}";
        _apiUrlTweet = $"{_apiUrlBase}/";
        _apiUrlDeleteTweet = $"{_apiUrlBase}/{{0}}";
        _apiUrlGetTweet = $"{_apiUrlBase}/{{0}}";
        _apiUrlGetUserTweets = $"{_apiUrlBase}/user/{{0}}";
        _apiUrlTimeline = $"{_apiUrlBase}/?since={{0}}&until={{1}}";

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
        GC.SuppressFinalize(this);
    }

    public int Add(DisplayTweet tweet)
    {
        Upsert(tweet);
        TimelineOrderByDescending();

        return _store.Timeline.Count;
    }

    public int AddRange(IEnumerable<DisplayTweet> tweets)
    {
        foreach (DisplayTweet tweet in tweets)
        {
            Upsert(tweet);
        }
        TimelineOrderByDescending();

        return _store.Timeline.Count;
    }

    public async ValueTask PostAsync(PostTweetContext postTweet)
    {
        HttpResponseMessage res = await _httpClient.PostAsJsonAsync(_apiUrlTweet, postTweet, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        res.EnsureSuccessStatusCode();
        DisplayTweet tweet = await res.Content.TwiHighReadFromJsonAsync<DisplayTweet>() ?? throw new TwiHighApiRequestException();
        Add(tweet);
        OnChangedTimeline?.Invoke();
        _messageService.SetSucessMessage("ツイートを送信しました！");
    }

    public async ValueTask<int> RemoveAsync(DisplayTweet tweet)
    {
        if (tweet.IsSystemTweet)
        {
            return RemoveTweetAtLocal(tweet.Id);
        }
        return await RemoveAsync(tweet.Id);
    }

    public async ValueTask<int> RemoveAsync(Guid tweetId)
    {
        await RemoveTweetAtServerAsync(tweetId);
        return RemoveTweetAtLocal(tweetId);
    }

    public async ValueTask CacheClearAsync()
    {
        _store = new LocalTimelineStore();
        await ForceSaveAsync();
    }

    public async ValueTask ForceSaveAsync(CancellationToken cancellationToken = default)
        => await _localStorageService.SetItemAsync(GetLocalStorageKeyUserTimeline(), _store.GetSaveData(), cancellationToken);

    public async ValueTask ForceLoadAsync(CancellationToken cancellationToken = default)
        => _store = await _localStorageService.GetItemAsync<LocalTimelineStore>(GetLocalStorageKeyUserTimeline(), cancellationToken);

    public async ValueTask ForceFetchMyTimelineAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken cancellationToken = default)
    {
        DisplayTweet[] tweets = await FetchMyTimelineAsync(since, until, cancellationToken).ConfigureAwait(false);
        AddRange(tweets);
        OnChangedTimeline?.Invoke();
    }

    public void MarkAsReadedTweets(IEnumerable<Guid> ids)
    {
        Guid[] tweetIds = ids.ToArray();
        foreach (Guid tweetId in tweetIds)
        {
            DisplayTweet? tweet = _store.Timeline.FirstOrDefault(t => t.Id == tweetId);
            if (tweet == null)
            {
                continue;
            }
            tweet.IsReaded = true;
        }
        OnChangedTimeline?.Invoke();
    }

    public async void Run() => await RunAsync().ConfigureAwait(false);

    public async void Stop() => await StopAsync().ConfigureAwait(false);
    #endregion

    #region private
    private async ValueTask<DisplayTweet[]> FetchMyTimelineAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken cancellationToken = default)
    {
        if (until < since)
        {
            throw new ArgumentException("開始時刻と終了時刻が逆転しています。", nameof(since));
        }
        string url = string.Format(
            _apiUrlTimeline,
            HttpUtility.UrlEncode(since.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
            HttpUtility.UrlEncode(until.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"))
        );
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return [];
        }
        ResponseTimelineContext context = await response.Content.TwiHighReadFromJsonAsync<ResponseTimelineContext>(cancellationToken: cancellationToken)
            ?? throw new TwiHighApiRequestException("タイムラインの取得でエラーが発生しました。");
        DisplayTweet[] tweets = DisplayTweet.ConvertFrom(context.Tweets);
        if (tweets.Length == 100)
        {
            DisplayTweet systemTweet = DisplayTweet.GetSystemTweet(context.Oldest);
            tweets = [.. tweets, systemTweet];
        }

        return tweets;
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

            // Check user id.
            string userId = await _authenticationStateProvider.GetLoggedInUserIdAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException($"未ログインで{nameof(TimelineWorkerService)}.{nameof(RunAsync)}()を実行することはできません。");
            }
            _store.UserId = Guid.Parse(userId);

            // Toggle the flag.
            _isRunning = true;

            // Load timeline data from local storage.
            await ForceLoadAsync(WorkerCancellationToken);

            // If cancellation requested or this instance disposed, break this loop.
            while (!WorkerCancellationToken.IsCancellationRequested && !_isDispose)
            {
                // Interval.
                await Task.Delay(5000, WorkerCancellationToken);

                // Do REST API.
                DisplayTweet[] tweets = await FetchMyTimelineAsync(_store.Latest.AddTicks(1), DateTimeOffset.MaxValue, WorkerCancellationToken).ConfigureAwait(false);
                AddRange(tweets);
                OnChangedTimeline?.Invoke();

                // Save timeline data to local storage.
                await ForceSaveAsync(WorkerCancellationToken);
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

    private void Upsert(DisplayTweet tweet)
    {
        int oldTweetIndex = _store.Timeline.FindIndex(t => t.Id == tweet.Id && t.UpdateAt < tweet.UpdateAt);
        if (0 <= oldTweetIndex)
        {
            _store.Timeline[oldTweetIndex] = tweet;
        }
    }

    private void TimelineOrderByDescending()
        => _store.Timeline = [.. _store.Timeline.OrderByDescending(x => x.CreateAt)];

    private string GetLocalStorageKeyUserTimeline()
    {
        if (_store.UserId == default)
        {
            throw new TwiHighException("ローカルストレージにユーザーIDが見つかりませんでした。");
        }

        return string.Format(LOCAL_STORAGE_KEY_USER_TIMELINE, _store.UserId);
    }

    private int RemoveTweetAtLocal(Guid tweetId)
    {
        DisplayTweet? targetTweet = _store.Timeline.Find(t => t.Id == tweetId);
        if (targetTweet != null)
        {
            _store.Timeline.Remove(targetTweet);
        }

        return _store.Timeline.Count;
    }

    private async Task RemoveTweetAtServerAsync(Guid tweetId)
    {
        try
        {
            string url = string.Format(_apiUrlDeleteTweet, tweetId.ToString());
            await _httpClient.DeleteAsync(url);
        }
        catch (HttpRequestException ex)
        {
            throw ex.StatusCode switch
            {
                HttpStatusCode.NotFound => new TwiHighApiRequestException("対象のツイートは既に削除されています。", httpRequestException: ex),
                HttpStatusCode.Unauthorized => new TwiHighApiRequestException("再度ログインしてから実行してください。", httpRequestException: ex),
                HttpStatusCode.Forbidden => new TwiHighApiRequestException("このツイートの削除は許可されていません。", httpRequestException: ex),
                _ => new TwiHighApiRequestException($"ツイート削除中にサーバでエラーが発生しました。({ex.StatusCode})", httpRequestException: ex),
            };
        }
        catch (Exception ex)
        {
            throw new TwiHighException($"ツイート削除中にサーバでエラーが発生しました。引き続きエラーが発生する場合は、サポートまでお問い合わせください。[{nameof(TimelineWorkerService)}.{nameof(RemoveTweetAtServerAsync)}()]", ex);
        }
    }

    private async void OnChangedAuthenticationState(Task<AuthenticationState> authenticationState)
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
            await ForceSaveAsync();
            _store = new()
            {
                UserId = userId
            };
            await ForceLoadAsync();
        }

        // Set new bearer token.
        string token = await _authenticationStateProvider.GetTokenFromLocalStorageAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
    #endregion
}
