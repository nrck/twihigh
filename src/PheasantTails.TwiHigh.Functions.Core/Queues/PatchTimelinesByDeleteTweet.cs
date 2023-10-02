namespace PheasantTails.TwiHigh.Functions.Core.Queues;

using Microsoft.Azure.Cosmos;
using PheasantTails.TwiHigh.Interface;

public class PatchTimelinesByDeleteTweet : ITimelinePatchOperationable
{
    private const string DELETE_TWEET_OVERRIDE_MESSAGE = "This tweet has been deleted.";
    public Guid TweetId { get; set; }
    public string SetText { get; set; } = DELETE_TWEET_OVERRIDE_MESSAGE;
    public bool SetIsDeleted { get; set; } = true;
    public DateTimeOffset SetUpdateAt { get; set; } = DateTimeOffset.UtcNow;

    public PatchOperation[] GetPatchOperations()
    {
        var operations = new[]
        {
            PatchOperation.Set("/text", SetText),
            PatchOperation.Set("/isDeleted", SetIsDeleted),
            PatchOperation.Set("/updateAt", SetUpdateAt)
        };
        return operations;
    }
}
