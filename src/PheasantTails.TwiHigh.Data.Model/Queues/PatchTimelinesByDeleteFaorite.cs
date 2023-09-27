namespace PheasantTails.TwiHigh.Data.Model.Queues;

using Microsoft.Azure.Cosmos;
using PheasantTails.TwiHigh.Interface;

public class PatchTimelinesByDeleteFaorite : ITimelinePatchOperationable
{
    public Guid TweetId { get; set; }
    public IEnumerable<IdTimeStampPair> ReplaceFavoriteFrom { get; set; } = Array.Empty<IdTimeStampPair>();
    public DateTimeOffset SetUpdateAt { get; set; } = DateTimeOffset.UtcNow;

    public PatchOperation[] GetPatchOperations()
    {
        var operations = new[]
        {
            PatchOperation.Replace("/favoriteFrom", ReplaceFavoriteFrom),
            PatchOperation.Set("/updateAt", SetUpdateAt)
        };
        return operations;
    }
}
