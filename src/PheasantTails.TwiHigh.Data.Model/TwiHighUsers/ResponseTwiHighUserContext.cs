using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Data.Model.TwiHighUsers
{
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

        public ResponseTwiHighUserContext(TwiHighUser user)
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
}
