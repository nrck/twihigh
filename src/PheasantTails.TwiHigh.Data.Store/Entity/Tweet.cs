namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    public class Tweet
    {
        public const string PARTITION_KEY = "/userId";

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserDisplayId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public Guid? ReplyTo { get; set; }
        public Guid[] ReplyFrom { get; set; } = Array.Empty<Guid>();
        public DateTimeOffset CreateAt { get; set; }
    }
}
