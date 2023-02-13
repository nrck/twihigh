using PheasantTails.TwiHigh.DataStore.Entity;

namespace PheasantTails.TwiHigh.Model.Timelines
{
    public class ResponseTimelineContext
    {
        public DateTimeOffset Latest { get; set; }
        public DateTimeOffset Oldest { get; set; }
        public Tweet[] Tweets { get; set; } = Array.Empty<Tweet>();
    }
}
