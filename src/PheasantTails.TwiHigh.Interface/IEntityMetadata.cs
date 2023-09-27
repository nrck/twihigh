namespace PheasantTails.TwiHigh.Interface;

public interface IEntityMetadata
{
    /// <summary>
    /// Cosmos DB item id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Updated time for this item.
    /// </summary>
    public DateTimeOffset UpdateAt { get; set; }

    /// <summary>
    /// Created time for this item.
    /// </summary>
    public DateTimeOffset CreateAt { get; set; }
}
