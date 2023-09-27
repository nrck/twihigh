using PheasantTails.TwiHigh.Interface;

namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    public class Tweet : BaseEntity, ITweet
    {
        public const string PARTITION_KEY = "/userId";

        public Guid UserId { get; set; }
        public string UserDisplayId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public Guid? ReplyTo { get; set; }
        public Guid[] ReplyFrom { get; set; } = Array.Empty<Guid>();
        public IdTimeStampPair[]? FavoriteFrom { get; set; } = Array.Empty<IdTimeStampPair>();
        public IdTimeStampPair[]? RetweetFrom { get; set; } = Array.Empty<IdTimeStampPair>();
        public override string GetPartitionKeyString() => UserId.ToString();
    }
}
