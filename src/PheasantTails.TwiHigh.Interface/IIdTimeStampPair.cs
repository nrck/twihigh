namespace PheasantTails.TwiHigh.Interface;

public interface IIdTimeStampPair
{
    /// <summary>
    /// Item's Id on Cosmos DB
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Created time of this pair.
    /// </summary>
    public DateTimeOffset TimeStamp { get; set; }
}
