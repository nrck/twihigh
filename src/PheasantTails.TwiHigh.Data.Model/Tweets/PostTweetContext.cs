namespace PheasantTails.TwiHigh.Data.Model.Tweets
{
    public class PostTweetContext
    {
        public string Text { get; set; } = string.Empty;
        public ReplyToContext? ReplyTo { get; set; }
    }

    public class ReplyToContext
    {
        public Guid TweetId { get; set; }
        public Guid UserId { get; set; }
    }
}