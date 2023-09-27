namespace PheasantTails.TwiHigh.Interface;

public interface IFeed : IEntityMetadata
{
    public const string FEED_TYPE_FAVORED = "Favored";
    public const string FEED_TYPE_RETWEETED = "Retweet";
    public const string FEED_TYPE_FOLLOWED = "Followed";
    public const string FEED_TYPE_MENTIONED = "Mentioned";
    public const string FEED_TYPE_INFORMATION = "Information";

    /// <summary>
    /// Feed type strings.
    /// </summary>
    public string FeedType { get; set; }

    /// <summary>
    /// User id of feeded to.
    /// </summary>
    public Guid FeedToUserId { get; set; }

    /// <summary>
    /// Tweet id of feeded to.<br />
    /// If <see cref="FeedType"/> is <see cref="FEED_TYPE_FOLLOWED"/> or <see cref="FEED_TYPE_INFORMATION"/>, this value is null.
    /// </summary>
    public Guid? FeedToTweetId { get; set; }

    /// <summary>
    /// User id of feed by.<br />
    /// If <see cref="FeedType"/> is <see cref="FEED_TYPE_INFORMATION"/>, this value is null.
    /// </summary>
    public Guid? FeedByUserId { get; set; }

    /// <summary>
    /// Tweet id of feed by.<br />
    /// If <see cref="FeedType"/> is NOT <see cref="FEED_TYPE_MENTIONED"/>, this value is null.
    /// </summary>
    public Guid? FeedByTweetId { get; set; }

    /// <summary>
    /// If <see cref="FeedType"/> is <see cref="FEED_TYPE_INFORMATION"/>, show this value on screen.<br />
    /// Else, this value is null.
    /// </summary>
    public string? InformationText { get; set; }

    /// <summary>
    /// If this feed is readed, this value is true.
    /// </summary>
    public bool IsOpened { get; set; }
}
