namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    /// <summary>
    /// Feedコンテナエンティティ
    /// </summary>
    public class Feed : BaseEntity
    {
        /// <summary>
        /// お気に入りされたときの通知
        /// </summary>
        public const string FEED_TYPE_FAVORED = "Favored";

        /// <summary>
        /// リツイートされたときの通知
        /// </summary>
        public const string FEED_TYPE_RETWEETED = "Retweet";

        /// <summary>
        /// フォローされたときの通知
        /// </summary>
        public const string FEED_TYPE_FOLLOWED = "Followed";

        /// <summary>
        /// メンションされたときの通知
        /// </summary>
        public const string FEED_TYPE_MENTIONED = "Mentioned";

        /// <summary>
        /// その他の通知
        /// </summary>
        public const string FEED_TYPE_INFORMATION = "Information";

        /// <summary>
        /// パーテーションキー
        /// </summary>
        public const string PARTITION_KEY = "/feedToUserId";

        /// <summary>
        /// このフィード先のアカウントデータIDの
        /// </summary>
        public Guid FeedToUserId { get; set; }

        /// <summary>
        /// 通知の種類。FEED_TYPEの何れかを設定する。
        /// </summary>
        public string FeedType { get; set; } = FEED_TYPE_INFORMATION;

        /// <summary>
        /// <see cref="FeedType"/>の操作をしたアカウントデータID<br />
        /// <see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
        /// </summary>
        public Guid? FeedByUserId { get; set; }

        /// <summary>
        /// <see cref="FeedType"/>の操作をしたアカウントID<br />
        /// <see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
        /// </summary>
        public string? FeedByUserDisplayId { get; set; }

        /// <summary>
        /// <see cref="FeedType"/>の操作をしたアカウント名<br />
        /// <see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
        /// </summary>
        public string? FeedByUserDisplayName { get; set; }

        /// <summary>
        /// <see cref="FeedType"/>の操作に関係するツイートデータID。<br />
        /// <see cref="FEED_TYPE_FOLLOWED"/>と<see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
        /// </summary>
        public Guid? ReferenceTweetId { get; set; }

        /// <summary>
        /// <see cref="FEED_TYPE_INFORMATION"/>のときに設定するテキストメッセージ。<br />
        /// <see cref="FEED_TYPE_INFORMATION"/>以外のときは<c>null</c>になる。
        /// </summary>
        public string? InformationText { get; set; }

        public Feed() { }

        /// <summary>
        /// お気に入りされたときの通知を作成する
        /// </summary>
        /// <param name="targetTweet">対象のツイート</param>
        /// <param name="feedByUser">お気に入りしたアカウント</param>
        /// <returns></returns>
        public static Feed CreateFavored(Tweet targetTweet, TwiHighUser feedByUser)
            => Create(targetTweet, feedByUser, FEED_TYPE_FAVORED);

        /// <summary>
        /// リツイートされたときの通知を作成する
        /// </summary>
        /// <param name="targetTweet">対象のツイート</param>
        /// <param name="feedByUser">リツイートしたアカウント</param>
        /// <returns></returns>
        public static Feed CreateRetweet(Tweet targetTweet, TwiHighUser feedByUser)
            => Create(targetTweet, feedByUser, FEED_TYPE_RETWEETED);

        /// <summary>
        /// リプライされたときの通知を作成する
        /// </summary>
        /// <param name="targetTweet">対象のツイート</param>
        /// <param name="feedByUser">リプライしたアカウント</param>
        /// <returns></returns>
        public static Feed CreateMentioned(Tweet targetTweet, TwiHighUser feedByUser)
            => Create(targetTweet, feedByUser, FEED_TYPE_MENTIONED);

        /// <summary>
        /// リプライされたときの通知を作成する
        /// </summary>
        /// <param name="targetUserId">対象のアカウントデータID</param>
        /// <param name="feedByUser">リプライしたアカウント</param>
        /// <returns></returns>
        public static Feed CreateMentioned(Guid targetUserId, TwiHighUser feedByUser)
        {
            var now = DateTimeOffset.UtcNow;
            var feed = new Feed
            {
                CreateAt = now,
                FeedByUserDisplayId = feedByUser.DisplayId,
                FeedByUserDisplayName = feedByUser.DisplayName,
                FeedByUserId = feedByUser.Id,
                FeedToUserId = targetUserId,
                FeedType = FEED_TYPE_MENTIONED,
                UpdateAt = now
            };

            return feed;
        }

        /// <summary>
        /// お知らせを作成する
        /// </summary>
        /// <param name="targetUserId">対象のアカウントデータID</param>
        /// <param name="text">お知らせメッセージ</param>
        /// <returns></returns>
        public static Feed Createinformation(Guid targetUserId, string text)
        {
            var now = DateTimeOffset.UtcNow;
            var feed = new Feed
            {
                CreateAt = now,
                FeedToUserId = targetUserId,
                FeedType = FEED_TYPE_INFORMATION,
                InformationText = text,
                UpdateAt = now
            };

            return feed;
        }

        private static Feed Create(Tweet targetTweet, TwiHighUser feedByUser, string type)
        {
            var now = DateTimeOffset.UtcNow;
            var feed = new Feed
            {
                CreateAt = now,
                FeedByUserDisplayId = feedByUser.DisplayId,
                FeedByUserDisplayName = feedByUser.DisplayName,
                FeedByUserId = feedByUser.Id,
                FeedToUserId = targetTweet.UserId,
                FeedType = type,
                ReferenceTweetId = targetTweet.Id,
                UpdateAt = now
            };

            return feed;
        }
    }
}
