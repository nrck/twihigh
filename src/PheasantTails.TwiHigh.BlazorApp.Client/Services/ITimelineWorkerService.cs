﻿using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public interface ITimelineWorkerService
{
    /// <summary>
    /// Loged in user's timeline.
    /// </summary>
    public ReadOnlyCollection<DisplayTweet> Timeline { get; }

    /// <summary>
    /// Adds a tweet to this instance's timeline.
    /// </summary>
    /// <param name="tweet">Tweet</param>
    /// <returns>This instance's timeline size after added.</returns>
    public int Add(DisplayTweet tweet);

    /// <summary>
    /// Adds tweets to this instance's timeline.
    /// </summary>
    /// <param name="tweets">Tweets</param>
    /// <returns>This instance's timeline size after added.</returns>
    public int AddRange(IEnumerable<DisplayTweet> tweets);

    /// <summary>
    /// Force to load timeline from local storage in your browser.
    /// </summary>
    public ValueTask ForceLoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Force to save timeline to local storage in your browser.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask ForceSaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tweet from this instance's timeline.
    /// </summary>
    /// <param name="tweet">Tweet</param>
    /// <returns>This instance's timeline size after deleted.</returns>
    public int Remove(DisplayTweet tweet);

    /// <summary>
    /// Deletes a tweet from this instance's timeline.
    /// </summary>
    /// <param name="tweet">Tweet</param>
    /// <returns>This instance's timeline size after deleted.</returns>
    public int Remove(Guid tweetId);
}