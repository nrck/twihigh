namespace PheasantTails.TwiHigh.Interface;

public interface IEntityMetadata : ICosmosDbItemId
{
    /// <summary>
    /// Updated time for this item.
    /// </summary>
    public DateTimeOffset UpdateAt { get; set; }

    /// <summary>
    /// Created time for this item.
    /// </summary>
    public DateTimeOffset CreateAt { get; set; }
}
