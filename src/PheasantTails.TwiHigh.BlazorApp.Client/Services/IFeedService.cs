using PheasantTails.TwiHigh.Data.Model.Feeds;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public interface IFeedService
{
    public event Action? NotifyChangedFeeds;
    public ObservableCollection<FeedContext> FeedContexts { get; set; }
    public int FeedDotCount { get; set; }

    public void Dispose();
    public ValueTask DisposeAsync();
    public Task InitializeAsync(string jwt);
    public Task MarkAsReadedFeedAsync(IEnumerable<Guid> ids);
}