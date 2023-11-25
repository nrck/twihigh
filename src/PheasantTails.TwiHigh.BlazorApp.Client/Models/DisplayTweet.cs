using PheasantTails.TwiHigh.Interface;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Models;

public class DisplayTweet : ITweet
{
    public string Text { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public Guid? ReplyTo { get; set; }
    public Guid[] ReplyFrom { get; set; } = Array.Empty<Guid>();
    public IdTimeStampPair[]? FavoriteFrom { get; set; }
    public IdTimeStampPair[]? RetweetFrom { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset UpdateAt { get; set; }
    public DateTimeOffset CreateAt { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string UserAvatarUrl { get; set; } = string.Empty;
}
