namespace PheasantTails.TwiHigh.Interface;

public interface ITwiHighUser : IEntityMetadata
{
    /// <summary>
    /// User id on screen.
    /// </summary>
    public string DisplayId { get; set; }

    /// <summary>
    /// Lower user id on screen.
    /// </summary>
    public string LowerDisplayId { get; set; }

    /// <summary>
    /// User name on screen.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Biography
    /// </summary>
    public string Biography { get; set; }

    /// <summary>
    /// This user's e-mail.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Count of this user's tweets.
    /// </summary>
    public long Tweets { get; set; }

    /// <summary>
    /// User icon url string.
    /// </summary>
    public string AvatarUrl { get; set; }

    /// <summary>
    /// Follows of this user.
    /// </summary>
    public Guid[] Follows { get; set; }

    /// <summary>
    /// Followers of this user.
    /// </summary>
    public Guid[] Followers { get; set; }
}
