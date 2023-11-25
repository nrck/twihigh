﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public class TimelineWorkerService : IAsyncDisposable, ITimelineWorkerService
{
    public const string LOCAL_STORAGE_KEY_USER_TIMELINE = "UserTimelines_{0}_v3";

    private LocalTimelineStore _store;
    private readonly ILocalStorageService _localStorageService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private bool _isDispose;
    private bool _isRunning;
    private CancellationTokenSource _cancellationTokenSource;

    public ReadOnlyCollection<DisplayTweet> Timeline => _store.Timeline.AsReadOnly<DisplayTweet>();
    private CancellationToken WorkerCancellationToken => _cancellationTokenSource.Token;

    public TimelineWorkerService(ILocalStorageService localStorageService, AuthenticationStateProvider authenticationStateProvider)
    {
        _store = new LocalTimelineStore();
        _localStorageService = localStorageService;
        _authenticationStateProvider = authenticationStateProvider;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    #region public
    public async ValueTask DisposeAsync()
    {
        if (_isDispose)
        {
            return;
        }
        _isDispose = true;
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
        foreach (var tweet in tweets)
        {
            Upsert(tweet);
        }
        TimelineOrderByDescending();

        return _store.Timeline.Count;
    }

    public int Remove(DisplayTweet tweet) => Remove(tweet.Id);

    public int Remove(Guid tweetId)
    {
        var targetTweet = _store.Timeline.Find(t => t.Id == tweetId);
        if (targetTweet != null)
        {
            _store.Timeline.Remove(targetTweet);
        }

        return _store.Timeline.Count;
    }

    public async ValueTask ForceSaveAsync(CancellationToken cancellationToken = default)
        // TODO
        => await SaveAsync(Guid.NewGuid(), cancellationToken);

    public async ValueTask ForceLoadAsync(CancellationToken cancellationToken = default)
        // TODO
        => await LoadAsync(Guid.NewGuid(), cancellationToken);
    #endregion

    #region private
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

            // Load timeline data from local storage.
            await ForceLoadAsync(WorkerCancellationToken);

            // If cancellation requested or this instance disposed, break this loop.
            while (!WorkerCancellationToken.IsCancellationRequested && !_isDispose)
            {
                // Interval.
                await Task.Delay(5000, WorkerCancellationToken);

                // TODO: Do REST API.
                // do something processing.

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
    }

    private async ValueTask SaveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _localStorageService.SetItemAsync(GetLocalTorageKeyUserTimeline(userId), _store.GetSaveData(), cancellationToken);
    }

    private async ValueTask LoadAsync(Guid userId, CancellationToken cancellationToken = default)
        => _store = await _localStorageService.GetItemAsync<LocalTimelineStore>(GetLocalTorageKeyUserTimeline(userId), cancellationToken);

    private void Upsert(DisplayTweet tweet)
    {
        var oldTweetIndex = _store.Timeline.FindIndex(t => t.Id == tweet.Id && t.UpdateAt < tweet.UpdateAt);
        if (0 <= oldTweetIndex)
        {
            _store.Timeline.RemoveAt(oldTweetIndex);
            _store.Timeline.Add(tweet);
        }
    }

    private void TimelineOrderByDescending()
    {
        _store.Timeline = [.. _store.Timeline.OrderByDescending(x => x.CreateAt)];
    }

    private string GetLocalTorageKeyUserTimeline(Guid userId)
        => string.Format(LOCAL_STORAGE_KEY_USER_TIMELINE, userId);
    #endregion
}