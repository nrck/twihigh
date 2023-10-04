namespace PheasantTails.TwiHigh.Functions.Core.Queues;

using PheasantTails.TwiHigh.Functions.Core.Entity;

public class FeedFavoredQueue
{
    public Guid TargetTweetId { get; set; }
    public string TargetTweetPartitionKey { get; set; } = string.Empty;
    public Guid FeedByUserId { get; set; }
    public string FeedByUserPartitionKey { get; set; } = string.Empty;

    public FeedFavoredQueue() { }

    public FeedFavoredQueue(Tweet targetTweet, TwiHighUser feedByUser)
    {
        TargetTweetId = targetTweet.Id;
        TargetTweetPartitionKey = targetTweet.UserId.ToString();
        FeedByUserId = feedByUser.Id;
        FeedByUserPartitionKey = feedByUser.Id.ToString();
    }
}
