namespace PheasantTails.TwiHigh.Data.Model.Feeds;

public class ResponseFeedsContext
{
    public DateTimeOffset Latest { get; set; }
    public DateTimeOffset Oldest { get; set; }
    public FeedContext[] Feeds { get; set; } = Array.Empty<FeedContext>();
}
