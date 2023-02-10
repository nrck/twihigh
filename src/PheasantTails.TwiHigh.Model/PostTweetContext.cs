namespace PheasantTails.TwiHigh.Model
{
    public class PostTweetContext
    {
        public string Text { get; set; } = string.Empty;
        public Guid? ReplyTo { get; set; }
    }
}