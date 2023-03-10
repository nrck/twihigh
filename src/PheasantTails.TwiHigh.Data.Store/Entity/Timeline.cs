namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    public class Timeline : BaseEntity
    {
        public const string PARTITION_KEY = "/ownerUserId";

        public Guid OwnerUserId { get; set; }
        public Guid TweetId { get; set; }
        public Guid UserId { get; set; }
        public string UserDisplayId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public Guid? ReplyTo { get; set; }
        public Guid[] ReplyFrom { get; set; } = Array.Empty<Guid>();

        public Timeline() { }

        public Timeline(Guid owner, Tweet tweet)
        {
            Id = Guid.NewGuid();
            TweetId = tweet.Id;
            OwnerUserId = owner;
            UserId = tweet.UserId;
            UserDisplayId = tweet.UserDisplayId;
            UserDisplayName = tweet.UserDisplayName;
            UserAvatarUrl = tweet.UserAvatarUrl;
            Text = tweet.Text;
            ReplyTo = tweet.ReplyTo;
            ReplyFrom = tweet.ReplyFrom;
            CreateAt = tweet.CreateAt;
        }

        public Tweet ToTweet()
        {
            var tweet = new Tweet
            {
                Id = TweetId,
                UserId = UserId,
                UserDisplayId = UserDisplayId,
                UserDisplayName = UserDisplayName,
                UserAvatarUrl = UserAvatarUrl,
                Text = Text,
                ReplyTo = ReplyTo,
                ReplyFrom = ReplyFrom,
                CreateAt = CreateAt
            };

            return tweet;
        }
    }
}
