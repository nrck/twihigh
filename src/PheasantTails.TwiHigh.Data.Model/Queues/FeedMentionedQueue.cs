using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class FeedMentionedQueue
    {
        public Guid TargetTweetId { get; set; }
        public string TargetTweetPartitionKey { get; set; } = string.Empty;
        public Guid FeedByUserId { get; set; }
        public string FeedByUserPartitionKey { get; set; } = string.Empty;
        public Guid FeedByTweetId { get; set; }
        public string FeedByTweetPartitionKey { get; set; } = string.Empty;

        public FeedMentionedQueue() { }

        public FeedMentionedQueue(Tweet targetTweet, TwiHighUser feedByUser, Tweet feedByTweet)
        {
            TargetTweetId = targetTweet.Id;
            TargetTweetPartitionKey = targetTweet.GetPartitionKeyString();
            FeedByUserId = feedByUser.Id;
            FeedByUserPartitionKey = feedByUser.GetPartitionKeyString();
            FeedByTweetId = feedByTweet.Id;
            FeedByTweetPartitionKey = feedByTweet.GetPartitionKeyString();
        }
    }
}
