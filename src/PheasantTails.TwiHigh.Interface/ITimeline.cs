namespace PheasantTails.TwiHigh.Interface;

public interface ITimeline : ITweet
{
    /// <summary>
    /// This timeline item owner.
    /// </summary>
    public Guid OwnerUserId { get; set; }

    /// <summary>
    /// This tweet id
    /// </summary>
    public Guid TweetId { get; set; }
}
