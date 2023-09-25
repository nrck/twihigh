using Microsoft.Azure.Cosmos;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class PatchTimelinesByRemoveReplyToQueue : ITimelinePatchOperationable
    {
        public Guid TweetId { get; set; }
        public DateTimeOffset SetUpdateAt { get; set; } = DateTimeOffset.UtcNow;

        public PatchOperation[] GetPatchOperations()
        {
            var operations = new[]
            {
                PatchOperation.Remove("/replyTo"),
                PatchOperation.Set("/updateAt", SetUpdateAt)
            };
            return operations;
        }
    }
}
