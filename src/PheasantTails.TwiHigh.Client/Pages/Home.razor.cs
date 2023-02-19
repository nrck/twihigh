using PheasantTails.TwiHigh.DataStore.Entity;
using PheasantTails.TwiHigh.Model.Timelines;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Home
    {
        private readonly ResponseTimelineContext TEST = new ResponseTimelineContext
        {
            Latest = DateTimeOffset.UtcNow,
            Oldest = DateTimeOffset.UtcNow,
            Tweets = new Tweet[]
            {
                new Tweet
                {
                    Id = Guid.NewGuid(),
                    CreateAt = DateTimeOffset.UtcNow,
                    ReplyFrom = Array.Empty<Guid>(),
                    ReplyTo = null,
                    Text = "テストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイート",
                    UserAvatarUrl = "https://pbs.twimg.com/profile_images/1547938967257251840/Y0IVpcxC_400x400.jpg",
                    UserDisplayId = "nr_ck",
                    UserDisplayName = "けー【新刊はBOOTH】",
                    UserId = Guid.NewGuid(),
                }
            }
        };

        private Tweet[] Tweets { get; set; }

        protected override Task OnInitializedAsync()
        {
            Tweets = TEST.Tweets;
            return base.OnInitializedAsync();
        }
    }
}
