namespace PheasantTails.TwiHigh.BlazorApp.Client.Models;

public class LocalTimelineStore
{
    public static int MaximumTweets => 10000;
    public DateTimeOffset Latest
    {
        get
        {
            if (0 < Timeline.Count)
            {
                return Timeline.Max(t => t.UpdateAt);
            }
            return DateTimeOffset.MinValue;
        }
    }

    public DateTimeOffset Oldest
    {
        get
        {
            if (0 < Timeline.Count)
            {
                return Timeline.Min(t => t.UpdateAt);
            }
            return DateTimeOffset.MinValue;
        }
    }

    public List<DisplayTweet> Timeline { get; set; } = [];
    public Guid UserId { get; set; }
    public LocalTimelineStore GetSaveData() => new()
    {
        UserId = UserId,
        Timeline = 0 < Timeline.Count ? Timeline.OrderByDescending(t => t.CreateAt)
            .Take(MaximumTweets)
            .ToList()
            : []
    };
}
