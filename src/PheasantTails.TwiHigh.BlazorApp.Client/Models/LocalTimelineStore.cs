namespace PheasantTails.TwiHigh.BlazorApp.Client.Models;

public class LocalTimelineStore
{
    public static int MaximumTweets => 10000;
    public DateTimeOffset Latest { get; set; }
    public DateTimeOffset Oldest { get; set; }
    public List<DisplayTweet> Timeline { get; set; } = [];
    public LocalTimelineStore GetSaveData() => new()
    {
        Latest = Latest,
        Oldest = Oldest,
        Timeline = Timeline.Take(MaximumTweets).ToList()
    };
}
