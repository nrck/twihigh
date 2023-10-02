namespace PheasantTails.TwiHigh.Functions.Core.Queues;
public class FeedMentionedQueue
{
    public Guid TargetTweetId { get; set; }
    public string TargetTweetPartitionKey { get; set; } = string.Empty;
    public Guid FeedByUserId { get; set; }
    public string FeedByUserPartitionKey { get; set; } = string.Empty;
    public Guid FeedByTweetId { get; set; }
    public string FeedByTweetPartitionKey { get; set; } = string.Empty;

    public FeedMentionedQueue() { }
}
