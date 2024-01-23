using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
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
    public ReactiveCommand<(ICosmosDbItemId Tweet, ITwiHighUserSummary User)> NavigateStatePageCommand { get; private set; } = default!;

    /// <summary>
    /// Navigate to user page command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> NavigateUserPageCommand { get; private set; } = default!;

    /// <summary>
    /// Navigate to reply form at state page command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> NavigateStatePageWithReplyCommand { get; private set; } = default!;

    public ReactiveCommand NavigateProfileEditorPageCommand { get; private set; } = default!;

    public AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; private set; } = default!;

    public HomeViewModel(HttpClient httpClient, IConfiguration configuration, ITimelineWorkerService timelineWorkerService, NavigationManager navigation, IMessageService messageService) : base(navigation, messageService)
    {
        // Inject
        _httpClient = httpClient;
        _apiUrlTweet = new($"{configuration["TweetApiUrl"]}/");
        _timelineWorkerService = timelineWorkerService;
    }

    protected override void Initialize()
    {
        AvatarUrl = new ReactivePropertySlim<string>(string.Empty).AddTo(_disposable);
        DeleteMyTweetCommand = new AsyncReactiveCommand<ICosmosDbItemId>().AddTo(_disposable);
        NavigateStatePageCommand = new ReactiveCommand<(ICosmosDbItemId Tweet, ITwiHighUserSummary User)>().AddTo(_disposable);
        NavigateUserPageCommand = new ReactiveCommand<ICosmosDbItemId>().AddTo(_disposable);
        NavigateStatePageWithReplyCommand = new ReactiveCommand<ICosmosDbItemId>().AddTo(_disposable);
        AddReactionCommand = new ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)>().AddTo(_disposable);
        RemoveReactionCommand = new ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)>().AddTo(_disposable);
        AddRetweetCommand = new ReactiveCommand<ICosmosDbItemId>().AddTo(_disposable);
        RemoveRetweetCommand = new ReactiveCommand<ICosmosDbItemId>().AddTo(_disposable);
        NavigateProfileEditorPageCommand = new ReactiveCommand().AddTo(_disposable);
        PostTweetCommand = new AsyncReactiveCommand<PostTweetContext>().AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        DeleteMyTweetCommand.Subscribe(async tweet => await _timelineWorkerService.RemoveAsync(tweet.Id));
        NavigateStatePageCommand.Subscribe(args => _navigationManager.NavigateToStatePage(args.User.UserDisplayId, args.Tweet));
        NavigateProfileEditorPageCommand.Subscribe(() => _navigationManager.NavigateToProfileEditorPage());
        PostTweetCommand.Subscribe(PostTweetAsync);
    }

    private async Task PostTweetAsync(PostTweetContext postTweet)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync(_apiUrlTweet, postTweet, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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
}
