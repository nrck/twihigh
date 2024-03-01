using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class ProfileViewModel : ViewModelBase, IProfileViewModel
{
    private readonly ITimelineWorkerService _timelineWorkerService;
    private readonly TwiHighAuthenticationStateProvider _authenticationStateProvider;
    private readonly string _apiUrlGetTwihighUser;
    private readonly string _apiUrlGetUserTweets;
    private readonly string _apiUrlAddFollow;
    private readonly string _apiUrlRemoveFollow;
    private readonly HttpClient _httpClient;
    private List<DisplayTweet>? _tweets;

    public ReadOnlyCollection<DisplayTweet>? Tweets => _tweets?.AsReadOnly();
    public ReactivePropertySlim<ResponseTwiHighUserContext?> UserDisplayedOnScreen { get; private set; } = default!;
    public ReactivePropertySlim<bool> IsMyTwiHighUser { get; private set; } = default!;
    public ReactivePropertySlim<string> PageTitle { get; private set; } = default!;
    public ReactivePropertySlim<bool> CanExecuteFollowOrRemove { get; private set; } = default!;

    public AsyncReactiveCommand<DisplayTweet> DeleteMyTweetCommand { get; private set; } = default!;
    public ReactiveCommandSlim NavigateProfileEditorPageCommand { get; private set; } = default!;
    public AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; private set; } = default!;
    public ReactiveCommand<ITwiHighUserSummary> NavigateUserPageCommand { get; private set; } = default!;
    public ReactiveCommand<ITweet> NavigateStatePageCommand { get; private set; } = default!;
    public AsyncReactiveCommand<string?> GetUserTimelineCommand { get; private set; } = default!;
    public AsyncReactiveCommand FollowUserDisplayedOnScreenCommand { get; private set; } = default!;
    public AsyncReactiveCommand RemoveUserDisplayedOnScreenCommand { get; private set; } = default!;

    public ProfileViewModel(HttpClient httpClient, IConfiguration configuration, AuthenticationStateProvider authenticationStateProvider, ITimelineWorkerService timelineWorkerService, NavigationManager navigationManager, IMessageService messageService)
        : base(navigationManager, messageService)
    {
        _httpClient = httpClient;
        _apiUrlGetTwihighUser = $"{configuration["AppUserApiUrl"]}/TwiHighUser/{{0}}";
        _apiUrlGetUserTweets = $"{configuration["TweetApiUrl"]}/user/{{0}}";
        _apiUrlAddFollow = $"{configuration["FollowApiUrl"]}/{{0}}";
        _apiUrlRemoveFollow = $"{configuration["FollowApiUrl"]}/{{0}}";
        _timelineWorkerService = timelineWorkerService;
        _authenticationStateProvider = authenticationStateProvider as TwiHighAuthenticationStateProvider
            ?? throw new ArgumentException($"{nameof(authenticationStateProvider)} is not {nameof(TwiHighAuthenticationStateProvider)}", nameof(authenticationStateProvider));
    }

    protected override void Initialize()
    {
        // property initialize
        UserDisplayedOnScreen = new ReactivePropertySlim<ResponseTwiHighUserContext?>().AddTo(_disposable);
        IsMyTwiHighUser = new ReactivePropertySlim<bool>().AddTo(_disposable);
        PageTitle = new ReactivePropertySlim<string>("プロフィール読み込み中").AddTo(_disposable);
        CanExecuteFollowOrRemove = new ReactivePropertySlim<bool>(true).AddTo(_disposable);

        // command initialize
        DeleteMyTweetCommand = new AsyncReactiveCommand<DisplayTweet>().AddTo(_disposable);
        NavigateProfileEditorPageCommand = new ReactiveCommandSlim().AddTo(_disposable);
        PostTweetCommand = new AsyncReactiveCommand<PostTweetContext>().AddTo(_disposable);
        NavigateUserPageCommand = new ReactiveCommand<ITwiHighUserSummary>().AddTo(_disposable);
        NavigateStatePageCommand = new ReactiveCommand<ITweet>().AddTo(_disposable);
        GetUserTimelineCommand = new AsyncReactiveCommand<string?>().AddTo(_disposable);
        FollowUserDisplayedOnScreenCommand = new AsyncReactiveCommand(CanExecuteFollowOrRemove).AddTo(_disposable);
        RemoveUserDisplayedOnScreenCommand = new AsyncReactiveCommand(CanExecuteFollowOrRemove).AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        DeleteMyTweetCommand.Subscribe(async tweet => await _timelineWorkerService.RemoveAsync(tweet.Id));
        NavigateProfileEditorPageCommand.Subscribe(() => _navigationManager.NavigateToProfileEditorPage());
        PostTweetCommand.Subscribe(async tweet => await _timelineWorkerService.PostAsync(tweet));
        NavigateUserPageCommand.Subscribe(tweet => _navigationManager.NavigateToProfilePage(tweet));
        NavigateStatePageCommand.Subscribe(tweet => _navigationManager.NavigateToStatePage(tweet));
        GetUserTimelineCommand.Subscribe(GetUserTimelineAsync);
        FollowUserDisplayedOnScreenCommand.Subscribe(FollowUserDisplayedOnScreenAsync);
        RemoveUserDisplayedOnScreenCommand.Subscribe(RemoveUserDisplayedOnScreenAsync);
    }

    private async Task GetUserTimelineAsync(string? id)
    {
        UserDisplayedOnScreen.Value = null;
        _tweets = null;
        IsMyTwiHighUser.Value = false;
        string myTwiHighUserId = id ?? await _authenticationStateProvider.GetLoggedInUserIdAsync().ConfigureAwait(false);
        string urlGetTwihighUser = string.Format(_apiUrlGetTwihighUser, myTwiHighUserId);
        try
        {
            UserDisplayedOnScreen.Value = await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext>(urlGetTwihighUser).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            UserDisplayedOnScreen.Value = null;
        }
        if (UserDisplayedOnScreen.Value == null)
        {
            PageTitle.Value = "プロフィールを読み込めませんでした。";
            _tweets = [];
            return;
        }

        PageTitle.Value = $"{UserDisplayedOnScreen.Value.DisplayName}（@{UserDisplayedOnScreen.Value.DisplayId}）";
        string urlGetUserTweets = string.Format(_apiUrlGetUserTweets, UserDisplayedOnScreen.Value.Id);
        HttpResponseMessage? res = await _httpClient.GetAsync(urlGetUserTweets);
        if (res != null && res.IsSuccessStatusCode && res.StatusCode == HttpStatusCode.OK)
        {
            ITweet[]? tweets = await res.Content.TwiHighReadFromJsonAsync<ITweet[]>();
            _tweets = tweets!.Select(tweet => new DisplayTweet(tweet) { IsReaded = true })
                .OrderByDescending(tweet => tweet.CreateAt)
                .ToList();
        }
        else
        {
            _tweets = [];
        }

        if (UserDisplayedOnScreen.Value.DisplayId != id)
        {
            // GuidからDisplayIdのページへ遷移
            _navigationManager.NavigateToProfilePage(UserDisplayedOnScreen.Value.DisplayId, replace: true);
        }
    }

    private async Task FollowUserDisplayedOnScreenAsync()
    {
        if (UserDisplayedOnScreen.Value == null)
        {
            return;
        }

        try
        {
            await SetAuthenticationHeaderValue().ConfigureAwait(false);
            string url = string.Format(_apiUrlAddFollow, UserDisplayedOnScreen.Value.Id);
            HttpResponseMessage res = await _httpClient.PutAsync(url, null);
            res.EnsureSuccessStatusCode();
            _messageService.SetSucessMessage($"@{UserDisplayedOnScreen.Value.DisplayId}さんをフォローしました！");
            string urlGetTwihighUser = string.Format(_apiUrlGetTwihighUser, UserDisplayedOnScreen.Value.Id);
            UserDisplayedOnScreen.Value = await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext>(urlGetTwihighUser).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            _messageService.SetErrorMessage("フォローできませんでした。");
        }
    }

    private async Task RemoveUserDisplayedOnScreenAsync()
    {
        if (UserDisplayedOnScreen.Value == null)
        {
            return;
        }

        try
        {
            await SetAuthenticationHeaderValue().ConfigureAwait(false);
            string url = string.Format(_apiUrlRemoveFollow, UserDisplayedOnScreen.Value.Id);
            HttpResponseMessage res = await _httpClient.DeleteAsync(url);
            res.EnsureSuccessStatusCode();
            _messageService.SetInfoMessage($"@{UserDisplayedOnScreen.Value.DisplayId}さんをリムーブしました！");
            string urlGetTwihighUser = string.Format(_apiUrlGetTwihighUser, UserDisplayedOnScreen.Value.Id);
            UserDisplayedOnScreen.Value = await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext>(urlGetTwihighUser).ConfigureAwait(false);

        }
        catch (HttpRequestException)
        {
            _messageService.SetErrorMessage("リムーブできませんでした。");
        }
    }

    private async Task SetAuthenticationHeaderValue()
    {
        string token = await _authenticationStateProvider.GetTokenFromLocalStorageAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
