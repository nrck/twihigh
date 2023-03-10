using PheasantTails.TwiHigh.DataStore.Entity;

namespace PheasantTails.TwiHigh.Model.Timelines
{
    public class QueAddTimelineContext
    {
        public Tweet Tweet{get;set;}
        public Guid[] Followers { get;set;}

        public QueAddTimelineContext(Tweet tweet, Guid[] followers)
        {
            Tweet = tweet;
            Followers = followers;
        }
    }
}
