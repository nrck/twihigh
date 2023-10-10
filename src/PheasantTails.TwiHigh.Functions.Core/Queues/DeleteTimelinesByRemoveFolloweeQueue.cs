namespace PheasantTails.TwiHigh.Functions.Core.Queues;

public class DeleteTimelinesByRemoveFolloweeQueue
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
