namespace PheasantTails.TwiHigh.Data.Model.Followers;

public class RemoveFolloweeTweetContext
{
    /// <summary>
    /// リムーブ対象のフォローユーザID
    /// </summary>
    public Guid FolloweeId { get; set; }

    /// <summary>
    /// 操作した人のユーザID
    /// </summary>
    public Guid UserId { get; set; }
}
