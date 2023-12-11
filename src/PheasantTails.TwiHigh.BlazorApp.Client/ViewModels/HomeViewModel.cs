﻿using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

/// <summary>
/// ViewModel for Home page.
/// </summary>
internal class HomeViewModel : IDisposable, INotifyCollectionChanged
{
    private readonly ITimelineWorkerService _timelineWorkerService;
    private readonly CompositeDisposable _disposable = [];
    private readonly NavigationManager _navigationManager;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Delete my tweet command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> DeleteMyTweetCommand { get; }

    /// <summary>
    /// Add reaction command.
    /// </summary>
    public ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)> AddReactionCommand { get; }

    /// <summary>
    /// Delete reaction command.
    /// </summary>
    public ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)> RemoveReactionCommand { get; }

    /// <summary>
    /// Retweet command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> AddRetweetCommand { get; }

    /// <summary>
    /// Remove retweet command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> RemoveRetweetCommand { get; }

    /// <summary>
    /// Navigate to state page command.
    /// </summary>
    public ReactiveCommand<(ICosmosDbItemId Tweet, ITwiHighUserSummary User)> NavigateStatePageCommand { get; }

    /// <summary>
    /// Navigate to user page command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> NavigateUserPageCommad { get; }

    /// <summary>
    /// Navigate to reply form at state page command.
    /// </summary>
    public ReactiveCommand<ICosmosDbItemId> NavigateStatePageWithReplyCommand { get; }

    public HomeViewModel(ITimelineWorkerService timelineWorkerService, NavigationManager navigation)
    {
        // Inject
        _timelineWorkerService = timelineWorkerService;
        _navigationManager = navigation;

        // Initialize
        DeleteMyTweetCommand = new ReactiveCommand<ICosmosDbItemId>();
        NavigateStatePageCommand = new ReactiveCommand<(ICosmosDbItemId Tweet, ITwiHighUserSummary User)>();
        NavigateUserPageCommad = new ReactiveCommand<ICosmosDbItemId>();
        NavigateStatePageWithReplyCommand = new ReactiveCommand<ICosmosDbItemId>();
        AddReactionCommand = new ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)>();
        RemoveReactionCommand = new ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)>();
        AddRetweetCommand = new ReactiveCommand<ICosmosDbItemId>();
        RemoveRetweetCommand = new ReactiveCommand<ICosmosDbItemId>();

        // Setting subscribe
        DeleteMyTweetCommand
            .Subscribe(async tweet => await _timelineWorkerService.RemoveAsync(tweet.Id))
            .AddTo(_disposable);
        NavigateStatePageCommand
            .Subscribe(args => _navigationManager.NavigateToStatePage(args.User.UserDisplayId, args.Tweet))
            .AddTo(_disposable);
    }

    public void Dispose() => _disposable.Dispose();
}
