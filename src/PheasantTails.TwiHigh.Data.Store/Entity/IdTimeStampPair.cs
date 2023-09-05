namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    public class IdTimeStampPair
    {
        public Guid Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
