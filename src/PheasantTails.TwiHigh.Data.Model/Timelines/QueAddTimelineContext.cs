namespace PheasantTails.TwiHigh.Data.Model.Timelines;

using PheasantTails.TwiHigh.Interface;

public class QueAddTimelineContext
{
    public ITweet Tweet { get; set; }
    public Guid[] Followers { get; set; }

    public QueAddTimelineContext(ITweet tweet, Guid[] followers)
    {
        Tweet = tweet;
        Followers = followers;
    }
}
