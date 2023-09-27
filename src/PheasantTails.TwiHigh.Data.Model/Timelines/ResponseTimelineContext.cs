namespace PheasantTails.TwiHigh.Data.Model.Timelines;

using PheasantTails.TwiHigh.Interface;

public class ResponseTimelineContext
{
    public DateTimeOffset Latest { get; set; }
    public DateTimeOffset Oldest { get; set; }
    public ITweet[] Tweets { get; set; } = Array.Empty<ITweet>();
}
