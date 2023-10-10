namespace PheasantTails.TwiHigh.Functions.Core.Queues;

using PheasantTails.TwiHigh.Functions.Core.Entity;

public class AddTimelineByPostTweetQueue
{
    public Tweet Tweet { get; set; }
    public Guid[] Followers { get; set; }

    public AddTimelineByPostTweetQueue(Tweet tweet, Guid[] followers)
    {
        Tweet = tweet;
        Followers = followers;
    }
}
