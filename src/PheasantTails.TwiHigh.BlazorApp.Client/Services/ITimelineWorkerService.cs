using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public interface ITimelineWorkerService : IAsyncDisposable
{
    /// <summary>
    /// Loged in user's timeline.
    /// </summary>
    public ReadOnlyCollection<DisplayTweet> Timeline { get; }

    event Action? OnChangedTimeline;

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
    ValueTask CacheClearAsync();

    /// <summary>
    /// Force to fetch timeline from TwiHigh server.
    /// </summary>
    public ValueTask ForceFetchMyTimelineAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken cancellationToken = default);

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
    /// Marks tweets as readed.
    /// </summary>
    void MarkAsReadedTweets(IEnumerable<Guid> ids);
    ValueTask PostAsync(PostTweetContext postTweet);

    /// <summary>
    /// Deletes a tweet from this instance's timeline.
    /// </summary>
    /// <param name="tweet">Tweet</param>
    /// <returns>This instance's timeline size after deleted.</returns>
    public ValueTask<int> RemoveAsync(DisplayTweet tweet);

    /// <summary>
    /// Deletes a tweet from this instance's timeline.
    /// </summary>
    /// <param name="tweet">Tweet</param>
    /// <returns>This instance's timeline size after deleted.</returns>
    public ValueTask<int> RemoveAsync(Guid tweetId);
    void Run();
    void Stop();
}