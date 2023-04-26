using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class UpdateTimelineQueue
    {
        public Tweet Tweet { get; set; }

        public UpdateTimelineQueue(Tweet tweet)
        {
            Tweet = tweet;
        }
    }
}
