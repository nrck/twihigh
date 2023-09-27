namespace PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

using PheasantTails.TwiHigh.Interface;

public class ResponseTwiHighUserContext
{
    public Guid Id { get; set; }
    public string DisplayId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public long Tweets { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public Guid[] Follows { get; set; } = Array.Empty<Guid>();
    public Guid[] Followers { get; set; } = Array.Empty<Guid>();
    public DateTimeOffset CreateAt { get; set; }

    public ResponseTwiHighUserContext() { }

    public ResponseTwiHighUserContext(ITwiHighUser user)
    {
        Id = user.Id;
        DisplayId = user.DisplayId;
        DisplayName = user.DisplayName;
        Biography = user.Biography;
        Follows = user.Follows;
        Followers = user.Followers;
        Tweets = user.Tweets;
        AvatarUrl = user.AvatarUrl;
        CreateAt = user.CreateAt;
    }
}
