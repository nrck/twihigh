namespace PheasantTails.TwiHigh.Functions.Core.Queues;

using Microsoft.Azure.Cosmos;
using PheasantTails.TwiHigh.Interface;

public class PatchTimelinesByUpdateUserInfoQueue : ITimelinePatchOperationable
{
    public Guid TweetId { get; set; }
    public string SetUserDisplayId { get; set; } = string.Empty;
    public string SetUserDisplayName { get; set; } = string.Empty;
    public string SetUserAvatarUrl { get; set; } = string.Empty;
    public DateTimeOffset SetUpdateAt { get; set; } = DateTimeOffset.UtcNow;

    public PatchOperation[] GetPatchOperations()
    {
        var operations = new[]
        {
            PatchOperation.Set("/userDisplayId", SetUserDisplayId),
            PatchOperation.Set("/userDisplayName", SetUserDisplayName),
            PatchOperation.Set("/userAvatarUrl", SetUserAvatarUrl),
            PatchOperation.Set("/updateAt", SetUpdateAt)
        };

        return operations;
    }
}
