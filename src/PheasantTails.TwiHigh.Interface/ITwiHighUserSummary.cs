namespace PheasantTails.TwiHigh.Interface;

public interface ITwiHighUserSummary
{
    /// <summary>
    /// User item id on Cosmos DB.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User id on screen.
    /// </summary>
    public string UserDisplayId { get; set; }

    /// <summary>
    /// User name on screen.
    /// </summary>
    public string UserDisplayName { get; set; }

    /// <summary>
    /// User icon url string.
    /// </summary>
    public string UserAvatarUrl { get; set; }
}
