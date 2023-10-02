namespace PheasantTails.TwiHigh.Functions.Core.Queues;

using Microsoft.Azure.Cosmos;
using PheasantTails.TwiHigh.Interface;

public class PatchTimelinesByAddFavoriteFromQueue : ITimelinePatchOperationable
{
    public Guid TweetId { get; set; }
    public IdTimeStampPair AddOrSetFavoriteFrom { get; set; } = new IdTimeStampPair();
    public bool IsAddOperation { get; set; }
    public DateTimeOffset SetUpdateAt { get; set; } = DateTimeOffset.UtcNow;

    public PatchOperation[] GetPatchOperations()
    {
        PatchOperation[] operations;
        if (IsAddOperation)
        {
            operations = new[]
            {
                PatchOperation.Add("/favoriteFrom/-", AddOrSetFavoriteFrom),
                PatchOperation.Set("/updateAt", SetUpdateAt)
            };
        }
        else
        {
            operations = new[]
            {
                PatchOperation.Set("/favoriteFrom", new[] { AddOrSetFavoriteFrom }),
                PatchOperation.Set("/updateAt", SetUpdateAt)
            };
        }

        return operations;
    }
}
