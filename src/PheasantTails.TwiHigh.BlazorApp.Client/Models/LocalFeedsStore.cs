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
    public DateTimeOffset Latest { get; set; }

    /// <summary>
    /// Oldest feed datetime in local storage.
    /// </summary>
    public DateTimeOffset Oldest { get; set; }

    /// <summary>
    /// Feeds timeline.
    /// </summary>
    public List<FeedContext> FeedTimeline { get; set; } = [];

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
        Latest = Latest,
        Oldest = Oldest,
        FeedTimeline = 0 < FeedTimeline.Count ? FeedTimeline.OrderByDescending(t => t.CreateAt)
            .Take(MaximumFeeds)
            .ToList()
            : []
    };
}
