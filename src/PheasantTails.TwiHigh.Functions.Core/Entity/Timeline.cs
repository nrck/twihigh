using PheasantTails.TwiHigh.Interface;

namespace PheasantTails.TwiHigh.Functions.Core.Entity
{
    /// <summary>
    /// Timelineコンテナエンティティ
    /// </summary>
    public class Timeline : BaseEntity, ITimeline
    {
        /// <summary>
        /// パーテーションキー
        /// </summary>
        public const string PARTITION_KEY = "/ownerUserId";

        public Guid OwnerUserId { get; set; }
        public Guid TweetId { get; set; }
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

        public Timeline() { }

        public Timeline(Guid owner, ITweet tweet)
        {
            Id = Guid.NewGuid();
            TweetId = tweet.Id;
            OwnerUserId = owner;
            UserId = tweet.UserId;
            UserDisplayId = tweet.UserDisplayId;
            UserDisplayName = tweet.UserDisplayName;
            UserAvatarUrl = tweet.UserAvatarUrl;
            Text = tweet.Text;
            IsDeleted = tweet.IsDeleted;
            ReplyTo = tweet.ReplyTo;
            ReplyFrom = tweet.ReplyFrom;
            UpdateAt = tweet.UpdateAt;
            CreateAt = tweet.CreateAt;
            FavoriteFrom = tweet.FavoriteFrom;
            RetweetFrom = tweet.RetweetFrom;
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
                IsDeleted = IsDeleted,
                UpdateAt = UpdateAt,
                CreateAt = CreateAt,
                FavoriteFrom = FavoriteFrom,
                RetweetFrom = RetweetFrom
            };

            return tweet;
        }

        public override string GetPartitionKeyString() => OwnerUserId.ToString();
    }
}
