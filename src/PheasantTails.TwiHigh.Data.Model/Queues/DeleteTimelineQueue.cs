using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class DeleteTimelineQueue
    {
        public Tweet Tweet { get; set; }

        public DeleteTimelineQueue(Tweet tweet)
        {
            Tweet = tweet;
        }
    }
}
