using PheasantTails.TwiHigh.Data.Model.Feeds;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Models;

public class DisplayFeed
{
    /// <summary>
    /// アイテムID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// データ更新日時
    /// </summary>
    public DateTimeOffset UpdateAt { get; set; }

    /// <summary>
    /// データ作成日時
    /// </summary>
    public DateTimeOffset CreateAt { get; set; }

    /// <summary>
    /// 通知の種類。FEED_TYPEの何れかを設定する。
    /// </summary>
    public string FeedType { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="FeedType"/>の操作をしたアカウント<br />
    /// <see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
    /// </summary>
    public ResponseTwiHighUserContext? FeedByUser { get; set; }

    /// <summary>
    /// <see cref="FeedType"/>の操作をしたアカウント<br />
    /// <see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
    /// </summary>
    public DisplayTweet? FeedByTweet { get; set; }

    /// <summary>
    /// <see cref="FeedType"/>の操作に関係するツイートデータID。<br />
    /// <see cref="FEED_TYPE_FOLLOWED"/>と<see cref="FEED_TYPE_INFORMATION"/>では<c>null</c>になる。
    /// </summary>
    public DisplayTweet? ReferenceTweet { get; set; }

    /// <summary>
    /// <see cref="FEED_TYPE_INFORMATION"/>のときに設定するテキストメッセージ。<br />
    /// <see cref="FEED_TYPE_INFORMATION"/>以外のときは<c>null</c>になる。
    /// </summary>
    public string? InformationText { get; set; }

    /// <summary>
    /// 既読フラグ
    /// </summary>
    public bool IsOpened { get; set; }

    public DisplayFeed() { }

    public DisplayFeed(FeedContext feedContext)
    {
        Id = feedContext.Id;
        UpdateAt = feedContext.UpdateAt;
        CreateAt = feedContext.CreateAt;
        FeedType = feedContext.FeedType;
        FeedByUser = feedContext.FeedByUser;
        FeedByTweet = feedContext.FeedByTweet == null ? null : new(feedContext.FeedByTweet);
        FeedByUser = feedContext.FeedByUser;
        InformationText = feedContext.InformationText;
        IsOpened = feedContext.IsOpened;
        ReferenceTweet = feedContext.ReferenceTweet == null ? null : new(feedContext.ReferenceTweet);
    }

    public static DisplayFeed[] ConvertFrom(IEnumerable<FeedContext> feeds)
    {

        DisplayFeed[] array = [];
        foreach (FeedContext feed in feeds)
        {
            DisplayFeed converted = new(feed);
            array = [.. array, converted];

        }
        return array;
    }
}
