using PheasantTails.TwiHigh.Interface;
using System.Text.Json.Serialization;

namespace PheasantTails.TwiHigh.Client.ViewModels
{
    public class TweetViewModel : ITweet
    {
        public static TweetViewModel SystemTweet => new() { IsSystemTweet = true };

        /// <summary>
        /// Cosmos DB item id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Updated time for this item.
        /// </summary>
        public DateTimeOffset UpdateAt { get; set; }

        /// <summary>
        /// Created time for this item.
        /// </summary>
        public DateTimeOffset CreateAt { get; set; }

        /// <summary>
        /// User item id on Cosmos DB.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User id on screen.
        /// </summary>
        public string UserDisplayId { get; set; } = string.Empty;

        /// <summary>
        /// User name on screen.
        /// </summary>
        public string UserDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// User icon url string.
        /// </summary>
        public string UserAvatarUrl { get; set; } = string.Empty;
        /// <summary>
        /// 既読管理フラグ（既読＝スクロール後に画面上に表示される）
        /// </summary>
        public bool IsReaded { get; set; }

        /// <summary>
        /// ギャップツイート取得用
        /// </summary>
        public bool IsSystemTweet { get; set; }

        /// <summary>
        /// ギャップツイート取得用
        /// </summary>
        public DateTimeOffset Since { get; set; }

        /// <summary>
        /// ギャップツイート取得用
        /// </summary>
        public DateTimeOffset Until { get; set; }

        /// <summary>
        /// 返信先
        /// </summary>
        public string ReplyToUserDisplayId { get; set; } = string.Empty;

        /// <summary>
        /// 強調表示する
        /// </summary>
        public bool IsEmphasized { get; set; }

        /// <summary>
        /// 投稿日時（文字列）
        /// </summary>
        [JsonIgnore]
        public string CreateAtDatetimeString
        {
            get
            {
                // 1年前ならyyyy/mm/dd
                if (CreateAt <= DateTimeOffset.UtcNow.AddYears(-1))
                {
                    return CreateAt.ToLocalTime().ToString("yyyy/MM/dd");
                }

                // 24時間より前ならm/d
                if (CreateAt <= DateTimeOffset.UtcNow.AddDays(-1))
                {
                    return CreateAt.ToLocalTime().ToString("M/d");
                }

                // 1時間より前なら H:mm
                if (CreateAt <= DateTimeOffset.UtcNow.AddHours(-1))
                {
                    return CreateAt.ToLocalTime().ToString("H:mm");
                }

                // H:mm:ss
                return CreateAt.ToLocalTime().ToString("H:mm:ss");
            }
        }
        public string Text { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public Guid? ReplyTo { get; set; }

        public Guid[] ReplyFrom { get; set; } = Array.Empty<Guid>();

        public IdTimeStampPair[]? FavoriteFrom { get; set; }

        public IdTimeStampPair[]? RetweetFrom { get; set; }

        public bool IsOpendReplyPostForm { get; set; } = false;

        public TweetViewModel(ITweet tweet)
        {
            CreateAt = tweet.CreateAt;
            FavoriteFrom = tweet.FavoriteFrom;
            Id = tweet.Id;
            IsDeleted = tweet.IsDeleted;
            IsEmphasized = false;
            IsReaded = false;
            ReplyFrom = tweet.ReplyFrom;
            ReplyTo = tweet.ReplyTo;
            RetweetFrom = tweet.RetweetFrom;
            Text = tweet.Text;
            UpdateAt = tweet.UpdateAt;
            UserAvatarUrl = tweet.UserAvatarUrl;
            UserDisplayId = tweet.UserDisplayId;
            UserDisplayName = tweet.UserDisplayName;
            UserId = tweet.UserId;
        }

        public TweetViewModel() { }
    }
}
