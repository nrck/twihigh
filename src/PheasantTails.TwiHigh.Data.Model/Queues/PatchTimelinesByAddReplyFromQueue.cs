using Microsoft.Azure.Cosmos;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class PatchTimelinesByAddReplyFromQueue : ITimelinePatchOperationable
    {
        public Guid TweetId { get; set; }
        public Guid AddReplyFrom { get; set; }
        public DateTimeOffset SetUpdateAt { get; set; } = DateTimeOffset.UtcNow;

        public PatchOperation[] GetPatchOperations()
        {
            var operations = new[]
            {
                PatchOperation.Add("/replyFrom/-", AddReplyFrom),
                PatchOperation.Set("/updateAt", SetUpdateAt)
            };
            return operations;
        }
    }
}
