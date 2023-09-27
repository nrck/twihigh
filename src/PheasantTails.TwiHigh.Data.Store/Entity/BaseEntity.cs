namespace PheasantTails.TwiHigh.Data.Store.Entity;

using PheasantTails.TwiHigh.Interface;

public abstract class BaseEntity : IEntityMetadata, IGetablePartitionKey
{
    public virtual Guid Id { get; set; } = Guid.NewGuid();
    public virtual DateTimeOffset UpdateAt { get; set; } = DateTimeOffset.UtcNow;
    public virtual DateTimeOffset CreateAt { get; set; } = DateTimeOffset.UtcNow;
    public abstract string GetPartitionKeyString();
}
