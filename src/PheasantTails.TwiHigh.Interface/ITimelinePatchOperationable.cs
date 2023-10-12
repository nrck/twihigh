namespace PheasantTails.TwiHigh.Interface;

public interface ITimelinePatchOperationable : IPatchOperationable
{
    /// <summary>
    /// ID of tweet item on Cosmos DB
    /// </summary>
    public Guid TweetId { get; }
}
