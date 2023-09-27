namespace PheasantTails.TwiHigh.Data.Store.Entity;

using PheasantTails.TwiHigh.Interface;

public class TwiHighUser : BaseEntity, ITwiHighUser
{
    public const string PARTITION_KEY = "/id";

    public string DisplayId { get; set; } = string.Empty;
    public string LowerDisplayId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long Tweets { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public Guid[] Follows { get; set; } = Array.Empty<Guid>();
    public Guid[] Followers { get; set; } = Array.Empty<Guid>();
    public override string GetPartitionKeyString() => Id.ToString();
}
