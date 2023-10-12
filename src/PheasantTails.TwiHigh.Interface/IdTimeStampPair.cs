namespace PheasantTails.TwiHigh.Interface;

public class IdTimeStampPair : IIdTimeStampPair
{
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
}
