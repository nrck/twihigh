using PheasantTails.TwiHigh.Data.Model.Feeds;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Models;

/// <summary>
/// Feeds store in local storage.
/// </summary>
public class LocalFeedsStore
{
    /// <summary>
    /// Maximum feeds stored count.
    /// </summary>
    public static int MaximumFeeds => 10000;

    /// <summary>
    /// Latest feed datetime in local storage.
    /// </summary>
    public DateTimeOffset Latest
    {
        get
        {
            if (0 < FeedTimeline.Count)
            {
                return FeedTimeline.Max(t => t.UpdateAt);
            }
            return DateTimeOffset.MinValue;
        }
    }

    /// <summary>
    /// Oldest feed datetime in local storage.
    /// </summary>
    public DateTimeOffset Oldest
    {
        get
        {
            if (0 < FeedTimeline.Count)
            {
                return FeedTimeline.Min(t => t.UpdateAt);
            }
            return DateTimeOffset.MinValue;
        }
    }

    /// <summary>
    /// Feeds timeline.
    /// </summary>
    public List<DisplayFeed> FeedTimeline { get; set; } = [];

    /// <summary>
    /// Owner user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Get save data.
    /// </summary>
    public LocalFeedsStore GetSaveData() => new()
    {
        UserId = UserId,
        FeedTimeline = 0 < FeedTimeline.Count ? FeedTimeline.OrderByDescending(t => t.CreateAt)
            .Take(MaximumFeeds)
            .ToList()
            : []
    };
}
