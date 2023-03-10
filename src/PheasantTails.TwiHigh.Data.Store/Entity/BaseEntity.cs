using Microsoft.Azure.Cosmos;

namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    public abstract class BaseEntity
    {

        public virtual Guid Id { get; set; } = Guid.Empty;
        public virtual DateTimeOffset CreateAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
