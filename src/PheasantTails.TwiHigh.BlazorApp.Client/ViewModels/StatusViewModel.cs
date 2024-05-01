using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class StatusViewModel : ViewModelBase, IStatusViewModel
{
    private readonly ITimelineWorkerService _timelineWorkerService;
    private readonly IJSRuntime _jSRuntime;
    private readonly string _apiUrlBase;
    private readonly string _apiUrlGetTweet;
    private readonly HttpClient _httpClient;

    private List<DisplayTweet>? _tweets;
    public ReadOnlyCollection<DisplayTweet>? Tweets => _tweets?.AsReadOnly();
    public ReactivePropertySlim<string> PageTitle { get; private set; } = default!;
    public ReactivePropertySlim<bool> CanScroll { get; private set; } = default!;

    public AsyncReactiveCommand<DisplayTweet> DeleteMyTweetCommand { get; private set; } = default!;
    public AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; private set; } = default!;
    public ReactiveCommandSlim NavigateProfileEditorPageCommand { get; private set; } = default!;
    public ReactiveCommandSlim<ITwiHighUserSummary> NavigateUserPageCommand { get; private set; } = default!;
    public ReactiveCommandSlim<DisplayTweet> NavigateStatePageCommand { get; private set; } = default!;
    public AsyncReactiveCommand<string> FetchTweetsCommand { get; private set; } = default!;
    public AsyncReactiveCommand<string> ScrollToTargetTweetCommand { get; private set; } = default!;

    public StatusViewModel(IJSRuntime jSRuntime, IHttpClientFactory httpClientFactory, IConfiguration configuration, ITimelineWorkerService timelineWorkerService, NavigationManager navigationManager, IMessageService messageService)
        : base(navigationManager, messageService)
    {
        _jSRuntime = jSRuntime;
        _timelineWorkerService = timelineWorkerService;
        _httpClient = httpClientFactory.CreateClient();
        _apiUrlBase = $"{configuration["TweetApiUrl"]}";
        _apiUrlGetTweet = $"{_apiUrlBase}/{{0}}";
    }

    protected override void Initialize()
    {
        PageTitle = new ReactivePropertySlim<string>("ツイートを読込中").AddTo(_disposable);
        CanScroll = new ReactivePropertySlim<bool>(true).AddTo(_disposable);
        DeleteMyTweetCommand = new AsyncReactiveCommand<DisplayTweet>().AddTo(_disposable);
        PostTweetCommand = new AsyncReactiveCommand<PostTweetContext>().AddTo(_disposable);
        NavigateProfileEditorPageCommand = new ReactiveCommandSlim().AddTo(_disposable);
        NavigateUserPageCommand = new ReactiveCommandSlim<ITwiHighUserSummary>().AddTo(_disposable);
        NavigateStatePageCommand = new ReactiveCommandSlim<DisplayTweet>().AddTo(_disposable);
        FetchTweetsCommand = new AsyncReactiveCommand<string>().AddTo(_disposable);
        ScrollToTargetTweetCommand = new AsyncReactiveCommand<string>(CanScroll).AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        DeleteMyTweetCommand.Subscribe(async tweet => await _timelineWorkerService.RemoveAsync(tweet.Id));
        PostTweetCommand.Subscribe(async tweet => await _timelineWorkerService.PostAsync(tweet));
        NavigateProfileEditorPageCommand.Subscribe(() => _navigationManager.NavigateToProfileEditorPage());
        NavigateUserPageCommand.Subscribe(user => _navigationManager.NavigateToProfilePage(user));
        NavigateStatePageCommand.Subscribe(tweet => _navigationManager.NavigateToStatePage(tweet));
        FetchTweetsCommand.Subscribe(FetchTweetsAsync);
        ScrollToTargetTweetCommand.Subscribe(ScrollToTargetTweetAsync);
    }

    private async Task FetchTweetsAsync(string id)
    {
        _tweets = null;
        PageTitle.Value = "ツイートを読込中";

        if (!Guid.TryParse(id, out Guid tweetId))
        {
            _messageService.SetWarnMessage("URLが間違っています。");
            _navigationManager.NavigateToHomePage(replace: true);
            return;
        }

        string url = string.Format(_apiUrlGetTweet, id);
        HttpResponseMessage res = await _httpClient.GetAsync(url).ConfigureAwait(false);
        if (!res.IsSuccessStatusCode)
        {
            _messageService.SetWarnMessage("指定されたツイートを取得できませんでした。");
            _navigationManager.NavigateToHomePage(replace: true);
            return;
        }

        ITweet[]? tweets = await res.Content.TwiHighReadFromJsonAsync<ITweet[]>();
        ITweet? main = tweets!.FirstOrDefault(t => t.Id == tweetId);
        if (main == null)
        {
            _messageService.SetWarnMessage("指定されたツイートは削除されています。");
            _navigationManager.NavigateToHomePage(replace: true);
            return;
        }

        PageTitle.Value = $"{main.UserDisplayName}さんのツイート：{main.Text}";
#pragma warning disable IDE0305 // コレクションの初期化を簡略化します
        _tweets = tweets!.Select(t =>
        {
            DisplayTweet tmp = new(t)
            {
                IsReaded = true
            };
            if (tmp.Id == tweetId)
            {
                tmp.IsEmphasized = true;
                tmp.IsOpendReplyPostForm = _navigationManager.Uri.ToLower().EndsWith("reply");
            }
            return tmp;
        }).OrderBy(t => t.CreateAt)
        .ToList();
#pragma warning restore IDE0305 // コレクションの初期化を簡略化します
    }

    private async Task ScrollToTargetTweetAsync(string tweetId)
    {
        await Task.Delay(300);
        string elementId = $"tweet-{tweetId}";
        await _jSRuntime.InvokeVoidAsync("BlazorScrollToId", elementId);
    }
}
