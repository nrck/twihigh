using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Data.Model.Feeds
{
    public class FeedContext
    {        /// <summary>
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
        /// アイテムID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// データ更新日時
        /// </summary>
        public DateTimeOffset UpdateAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// データ作成日時
        /// </summary>
        public DateTimeOffset CreateAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 通知の種類。FEED_TYPEの何れかを設定する。
        /// </summary>
        public string FeedType { get; set; } = FEED_TYPE_INFORMATION;

        /// <summary>
        /// <see cref="FeedType"/>の操作をしたアカウント<br />
        /// <see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
        /// </summary>
        public ResponseTwiHighUserContext? FeedByUser { get; set; }

        /// <summary>
        /// <see cref="FeedType"/>の操作に関係するツイートデータID。<br />
        /// <see cref="FEED_TYPE_FOLLOWED"/>と<see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
        /// </summary>
        public Tweet? ReferenceTweet { get; set; }

        /// <summary>
        /// <see cref="FEED_TYPE_INFORMATION"/>のときに設定するテキストメッセージ。<br />
        /// <see cref="FEED_TYPE_INFORMATION"/>以外のときは<c>null</c>になる。
        /// </summary>
        public string? InformationText { get; set; }

        /// <summary>
        /// 既読フラグ
        /// </summary>
        public bool IsOpened { get; set; }
    }
}
