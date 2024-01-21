using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

/// <summary>
/// ViewModel for Home page.
/// </summary>
public class HomeViewModel : ViewModelBase, IHomeViewModel
{
    private readonly ITimelineWorkerService _timelineWorkerService;

    /// <summary>
    /// Delete my tweet command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> DeleteMyTweetCommand { get; } = new ReactiveCommand<ICosmosDbItemId>();

    /// <summary>
    /// Add reaction command.
    /// </summary>
    public ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)> AddReactionCommand { get; } = new ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)>();

    /// <summary>
    /// Delete reaction command.
    /// </summary>
    public ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)> RemoveReactionCommand { get; } = new ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)>();

    /// <summary>
    /// Retweet command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> AddRetweetCommand { get; } = new ReactiveCommand<ICosmosDbItemId>();

    /// <summary>
    /// Remove retweet command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> RemoveRetweetCommand { get; } = new ReactiveCommand<ICosmosDbItemId>();

    /// <summary>
    /// Navigate to state page command.
    /// </summary>
    public ReactiveCommand<(ICosmosDbItemId Tweet, ITwiHighUserSummary User)> NavigateStatePageCommand { get; } = new ReactiveCommand<(ICosmosDbItemId Tweet, ITwiHighUserSummary User)>();

    /// <summary>
    /// Navigate to user page command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> NavigateUserPageCommad { get; } = new ReactiveCommand<ICosmosDbItemId>();

    /// <summary>
    /// Navigate to reply form at state page command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> NavigateStatePageWithReplyCommand { get; } = new ReactiveCommand<ICosmosDbItemId>();

    public HomeViewModel(ITimelineWorkerService timelineWorkerService, NavigationManager navigation, IMessageService messageService) : base(navigation, messageService)
    {
        // Inject
        _timelineWorkerService = timelineWorkerService;

        // Initialize
        DeleteMyTweetCommand.AddTo(_disposable);
        NavigateStatePageCommand.AddTo(_disposable);
        NavigateUserPageCommad.AddTo(_disposable);
        NavigateStatePageWithReplyCommand.AddTo(_disposable);
        AddReactionCommand.AddTo(_disposable);
        RemoveReactionCommand.AddTo(_disposable);
        AddRetweetCommand.AddTo(_disposable);
        RemoveRetweetCommand.AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        DeleteMyTweetCommand.Subscribe(async tweet => await _timelineWorkerService.RemoveAsync(tweet.Id));
        NavigateStatePageCommand.Subscribe(args => _navigationManager.NavigateToStatePage(args.User.UserDisplayId, args.Tweet));
    }
}
