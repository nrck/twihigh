namespace PheasantTails.TwiHigh.Interface;

public interface ITimelinePatchOperationable : IPatchOperationable
{
    public Guid TweetId { get; }
}
