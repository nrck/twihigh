namespace PheasantTails.TwiHigh.DataStore.Entity
{
    public class TwiHighUser
    {
        public const string PARTITION_KEY = "/id";

        public Guid Id { get; set; }
        public string DisplayId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTimeOffset CreateAt { get; set; } = DateTimeOffset.UtcNow;
        public long Tweets { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public Guid[] Follows { get; set; } = Array.Empty<Guid>();
        public Guid[] Followers { get; set; } = Array.Empty<Guid>();
    }
}
