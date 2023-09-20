using PheasantTails.TwiHigh.Data.Model.Feeds;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.Client.Services
{
    public interface IFeedService
    {
        public ObservableCollection<FeedContext> FeedContexts { get; set; }
        public int FeedDotCount { get; set; }
        public Action NotifyChangedFeeds { get; set; }

        public void Dispose();
        public ValueTask DisposeAsync();
        public Task InitializeAsync(string jwt);
    }
}