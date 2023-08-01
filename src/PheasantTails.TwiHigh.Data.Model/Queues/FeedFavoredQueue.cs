namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class FeedFavoredQueue
    {
        public Guid TargetTweetId { get; set; }
        public string TargetTweetPartitionKey { get; set; } = string.Empty;
        public Guid FeedByUserId { get; set; }
        public string FeedByUserPartitionKey { get; set; } = string.Empty;
    }
}
