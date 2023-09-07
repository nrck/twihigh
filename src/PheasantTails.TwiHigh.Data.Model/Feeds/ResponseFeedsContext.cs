using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Data.Model.Feeds
{
    public class ResponseFeedsContext
    {
        public DateTimeOffset Latest { get; set; }
        public DateTimeOffset Oldest { get; set; }
        public Feed[] Feeds { get; set; } = Array.Empty<Feed>();
    }
}
