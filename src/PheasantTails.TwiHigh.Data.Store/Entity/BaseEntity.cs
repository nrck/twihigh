namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    /// <summary>
    /// CosmosDB 基本クラス
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// アイテムID
        /// </summary>
        public virtual Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// データ更新日時
        /// </summary>
        public virtual DateTimeOffset UpdateAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// データ作成日時
        /// </summary>
        public virtual DateTimeOffset CreateAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
