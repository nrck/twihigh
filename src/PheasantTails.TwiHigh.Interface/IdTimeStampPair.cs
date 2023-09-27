namespace PheasantTails.TwiHigh.Interface;

public class IdTimeStampPair : IIdTimeStampPair
{
    public Guid Id { get; set; }
    public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
}
