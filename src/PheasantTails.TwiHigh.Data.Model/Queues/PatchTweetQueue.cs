using Microsoft.Azure.Cosmos;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class PatchTweetQueue
    {
        public Guid TweetId { get; set; }
        public PatchOperation[] Operations { get; set; } = Array.Empty<PatchOperation>();
    }
}
