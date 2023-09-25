namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public interface ITimelinePatchOperationable : IPatchOperationable
    {
        public Guid TweetId { get; }
    }
}
