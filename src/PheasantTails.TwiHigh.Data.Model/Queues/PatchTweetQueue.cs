using Microsoft.Azure.Cosmos;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class PatchTweetQueue
    {
        /// <summary>
        /// Target tweet id
        /// </summary>
        public Guid TweetId { get; set; }

        /// <summary>
        /// Patch operation
        /// </summary>
        public PatchOperation[] Operations { get; set; } = Array.Empty<PatchOperation>();
    }
}
