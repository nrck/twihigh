using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Text.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

/// <summary>
/// ViewModel for Home page.
/// </summary>
public class HomeViewModel : ViewModelBase, IHomeViewModel
{
    private readonly ITimelineWorkerService _timelineWorkerService;
    private readonly HttpClient _httpClient;
    private readonly Uri _apiUrlTweet;
    private readonly TwiHighAuthenticationStateProvider _authenticationStateProvider;

    public ReactivePropertySlim<string> AvatarUrl { get; private set; } = default!;

    /// <summary>
    /// Delete my tweet command.
    /// </summary>
    public AsyncReactiveCommand<ICosmosDbItemId> DeleteMyTweetCommand { get; private set; } = default!;

    /// <summary>
    /// Add reaction command.
    /// </summary>
    public ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)> AddReactionCommand { get; private set; } = default!;

    /// <summary>
    /// Delete reaction command.
    /// </summary>
    public ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)> RemoveReactionCommand { get; private set; } = default!;

    /// <summary>
    /// Retweet command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> AddRetweetCommand { get; private set; } = default!;

    /// <summary>
    /// Remove retweet command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> RemoveRetweetCommand { get; private set; } = default!;

    /// <summary>
    /// Navigate to state page command.
    /// </summary>
    public ReactiveCommand<ITweet> NavigateStatePageCommand { get; private set; } = default!;

    /// <summary>
    /// Navigate to user page command.
    /// </summary>
    public ReactiveCommand<ITwiHighUserSummary> NavigateUserPageCommand { get; private set; } = default!;

    /// <summary>
    /// Navigate to reply form at state page command.
    /// </summary>
    public ReactiveCommand<ITweet> NavigateStatePageWithReplyCommand { get; private set; } = default!;

    public ReactiveCommand NavigateProfileEditorPageCommand { get; private set; } = default!;

    public AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; private set; } = default!;

    public ReactivePropertySlim<Guid> MyTwiHighUserId { get; private set; } = default!;

    public AsyncReactiveCommand GetLoginUserIdCommand { get; private set; } = default!;

    public AsyncReactiveCommand<DisplayTweet> GetGapTweetCommand { get; private set; } = default!;

    public AsyncReactiveCommand GetMyAvatarUrlCommand { get; private set; } = default!;

    public ReactiveCommandSlim<IEnumerable<string>> MarkAsReadedTweetCommand { get; private set; } = default!;

    public ReactivePropertySlim<bool> IsProcessingMarkAsReaded { get; private set; } = default!;

    public HomeViewModel(AuthenticationStateProvider authenticationStateProvider, HttpClient httpClient, IConfiguration configuration, ITimelineWorkerService timelineWorkerService, NavigationManager navigation, IMessageService messageService)
        : base(navigation, messageService)
    {
        // Inject
        _httpClient = httpClient;
        _apiUrlTweet = new($"{configuration["TweetApiUrl"]}/");
        _timelineWorkerService = timelineWorkerService;
        _authenticationStateProvider = (TwiHighAuthenticationStateProvider)authenticationStateProvider;
    }

    protected override void Initialize()
    {
        AvatarUrl = new ReactivePropertySlim<string>(string.Empty).AddTo(_disposable);
        DeleteMyTweetCommand = new AsyncReactiveCommand<ICosmosDbItemId>().AddTo(_disposable);
        NavigateStatePageCommand = new ReactiveCommand<ITweet>().AddTo(_disposable);
        NavigateUserPageCommand = new ReactiveCommand<ITwiHighUserSummary>().AddTo(_disposable);
        NavigateStatePageWithReplyCommand = new ReactiveCommand<ITweet>().AddTo(_disposable);
        AddReactionCommand = new ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)>().AddTo(_disposable);
        RemoveReactionCommand = new ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)>().AddTo(_disposable);
        AddRetweetCommand = new ReactiveCommand<ICosmosDbItemId>().AddTo(_disposable);
        RemoveRetweetCommand = new ReactiveCommand<ICosmosDbItemId>().AddTo(_disposable);
        NavigateProfileEditorPageCommand = new ReactiveCommand().AddTo(_disposable);
        PostTweetCommand = new AsyncReactiveCommand<PostTweetContext>().AddTo(_disposable);
        MyTwiHighUserId = new ReactivePropertySlim<Guid>().AddTo(_disposable);
        GetLoginUserIdCommand = new AsyncReactiveCommand().AddTo(_disposable);
        GetGapTweetCommand = new AsyncReactiveCommand<DisplayTweet>().AddTo(_disposable);
        GetMyAvatarUrlCommand = new AsyncReactiveCommand().AddTo(_disposable);
        MarkAsReadedTweetCommand = new ReactiveCommandSlim<IEnumerable<string>>(IsProcessingMarkAsReaded).AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        DeleteMyTweetCommand.Subscribe(async tweet => await _timelineWorkerService.RemoveAsync(tweet.Id));
        NavigateStatePageCommand.Subscribe(tweet => _navigationManager.NavigateToStatePage(tweet));
        NavigateStatePageWithReplyCommand.Subscribe(tweet => _navigationManager.NavigateToStatePage(tweet, true));
        NavigateProfileEditorPageCommand.Subscribe(() => _navigationManager.NavigateToProfileEditorPage());
        NavigateUserPageCommand.Subscribe((t) => _navigationManager.NavigateToProfilePage(t));
        PostTweetCommand.Subscribe(PostTweetAsync);
        GetLoginUserIdCommand.Subscribe(SetMyTwiHighUserIdAsync);
        GetGapTweetCommand.Subscribe(GetGapTweetsAsync);
        GetMyAvatarUrlCommand.Subscribe(SetMyTwiHighUserAvatarUrlAsync);
        MarkAsReadedTweetCommand.Subscribe(MarkAsReadedTweets);
    }

    private async Task PostTweetAsync(PostTweetContext postTweet)
    {
        try
        {
            HttpResponseMessage res = await _httpClient.PostAsJsonAsync(_apiUrlTweet, postTweet, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            res.EnsureSuccessStatusCode();
            _messageService.SetSucessMessage("ツイートを送信しました！");
        }
        catch (Exception _)
        {
#if DEBUG
            HandleException(_);
#else
            _messageService.SetErrorMessage("ツイートできませんでした。");
#endif
        }
    }

    private async Task SetMyTwiHighUserIdAsync()
    {
        string id = await _authenticationStateProvider.GetLoggedInUserIdAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(id))
        {
            MyTwiHighUserId.Value = new Guid();
        }
        else
        {
            MyTwiHighUserId.Value = Guid.Parse(id);
        }
    }

    private async Task GetGapTweetsAsync(DisplayTweet tweet)
    {
        if (!tweet.IsSystemTweet)
        {
            return;
        }
        await _timelineWorkerService.ForceFetchMyTimelineAsync(tweet.Since, tweet.Until).ConfigureAwait(false);
        await _timelineWorkerService.RemoveAsync(tweet).ConfigureAwait(false);
        await _timelineWorkerService.ForceSaveAsync().ConfigureAwait(false);
    }

    private async Task SetMyTwiHighUserAvatarUrlAsync()
        => AvatarUrl.Value = await _authenticationStateProvider.GetLoggedInUserAvatarUrlAsync().ConfigureAwait(false);

    private void MarkAsReadedTweets(IEnumerable<string> ids)
        => _timelineWorkerService.MarkAsReadedTweets(ids.ToArray().Select(id => Guid.Parse(id[6..])).ToArray());
}
