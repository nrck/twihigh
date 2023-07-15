using PheasantTails.TwiHigh.Data.Store.Entity;
using System.Text.Json.Serialization;

namespace PheasantTails.TwiHigh.Client.ViewModels
{
    public class TweetViewModel : Tweet
    {
        public static TweetViewModel SystemTweet => new() { IsSystemTweet = true };

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

        public bool IsOpendReplyPostForm { get; set; } = false;

        public TweetViewModel(Tweet tweet)
        {
            CreateAt = tweet.CreateAt;
            Id = tweet.Id;
            IsDeleted = tweet.IsDeleted;
            IsReaded = false;
            ReplyFrom = tweet.ReplyFrom;
            ReplyTo = tweet.ReplyTo;
            Text = tweet.Text;
            UpdateAt = tweet.UpdateAt;
            UserAvatarUrl = tweet.UserAvatarUrl;
            UserDisplayId = tweet.UserDisplayId;
            UserDisplayName = tweet.UserDisplayName;
            UserId = tweet.UserId;
        }

        public TweetViewModel() { }

        public string GetTextWithNewline() => Text.TrimStart('\r', '\n').TrimEnd('\r', '\n').Replace(Environment.NewLine, "<br />");
    }
}
