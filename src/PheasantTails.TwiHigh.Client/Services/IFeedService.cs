using PheasantTails.TwiHigh.Data.Model.Feeds;

namespace PheasantTails.TwiHigh.Client.Services
{
    public interface IFeedService
    {
        public FeedContext[] FeedContexts { get; set; }
        public int FeedDotCount { get; set; }
        public Action NotifyChangedFeeds { get; set; }

        public void Dispose();
        public ValueTask DisposeAsync();
        public Task InitializeAsync(string jwt);
    }
}