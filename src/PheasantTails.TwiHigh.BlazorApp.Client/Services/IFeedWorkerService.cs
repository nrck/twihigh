using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public interface IFeedWorkerService : IAsyncDisposable
{
    ReadOnlyCollection<DisplayFeed> FeedTimeline { get; }

    event Action? OnChangedFeedTimeline;

    ValueTask CacheClearAsync();
    ValueTask ForceFetchMyFeedTimelineAsync(DateTimeOffset since, DateTimeOffset until, CancellationToken cancellationToken = default);
    ValueTask ForceLoadAsync(CancellationToken cancellationToken = default);
    ValueTask ForceSaveAsync(CancellationToken cancellationToken = default);
    Task MarkAsReadedFeedsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    void Run();
    void Stop();
}
