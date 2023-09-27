namespace PheasantTails.TwiHigh.Interface;

public interface ITweet : IEntityMetadata, IGetablePartitionKey, ITwiHighUserSummary
{
    /// <summary>
    /// This tweet's main text.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// If this tweet is deleted, <see cref="true"/>.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Tweet id reply to.
    /// </summary>
    public Guid? ReplyTo { get; set; }

    /// <summary>
    /// Tweet id reply from.
    /// </summary>
    public Guid[] ReplyFrom { get; set; }

    /// <summary>
    /// User id favorited from.
    /// </summary>
    public IdTimeStampPair[]? FavoriteFrom { get; set; }

    /// <summary>
    /// User id retweeted from.
    /// </summary>
    public IdTimeStampPair[]? RetweetFrom { get; set; }
}