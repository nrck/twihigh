namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    public abstract class BaseEntity
    {
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public virtual DateTimeOffset UpdateAt { get; set; } = DateTimeOffset.UtcNow;
        public virtual DateTimeOffset CreateAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
