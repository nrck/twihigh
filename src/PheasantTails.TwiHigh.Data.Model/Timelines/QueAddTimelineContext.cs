using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Data.Model.Timelines
{
    public class QueAddTimelineContext
    {
        public Tweet Tweet { get; set; }
        public Guid[] Followers { get; set; }

        public QueAddTimelineContext(Tweet tweet, Guid[] followers)
        {
            Tweet = tweet;
            Followers = followers;
        }
    }
}
