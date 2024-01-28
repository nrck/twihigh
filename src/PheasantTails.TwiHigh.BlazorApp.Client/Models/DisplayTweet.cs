using PheasantTails.TwiHigh.Interface;
using System.Text.Json.Serialization;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Models;

public class DisplayTweet : ITweet, ITwiHighUserSummary
{
    public string Text { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public Guid? ReplyTo { get; set; }
    public Guid[] ReplyFrom { get; set; } = [];
    public IdTimeStampPair[]? FavoriteFrom { get; set; }
    public IdTimeStampPair[]? RetweetFrom { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset UpdateAt { get; set; }
    public DateTimeOffset CreateAt { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string UserAvatarUrl { get; set; } = string.Empty;
    public bool IsReaded { get; set; }
    public bool IsSystemTweet { get; set; }
    public DateTimeOffset Since { get; set; }
    public DateTimeOffset Until { get; set; }
    public bool IsOpendReplyPostForm { get; set; }
    public bool IsEmphasized { get; set; }
    public string ReplyToUserDisplayId { get; set; } = string.Empty;
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
    public DisplayTweet() { }

    public DisplayTweet(ITweet tweet)
    {
        CreateAt = tweet.CreateAt;
        FavoriteFrom = tweet.FavoriteFrom;
        Id = tweet.Id;
        IsDeleted = tweet.IsDeleted;
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

    public static DisplayTweet[] ConvertFrom (IEnumerable<ITweet> tweets)
    {
        DisplayTweet[] array = [];
        foreach (ITweet tweet in tweets)
        {
            if(tweet is DisplayTweet displayTweet)
            {
                array = [.. array, displayTweet];
            }
            else
            {
                DisplayTweet converted = new(tweet);
                array = [.. array, converted];
            }
        }
        return array;
    }

    public static DisplayTweet GetSystemTweet(DateTimeOffset until)
        => new()
        {
            IsSystemTweet = true,
            Id = Guid.NewGuid(),
            Since = DateTimeOffset.MinValue,
            Until = until,
            CreateAt = until.AddTicks(-1),
            UpdateAt = until.AddTicks(-1)
        };
}
