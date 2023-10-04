namespace PheasantTails.TwiHigh.Functions.Core.Queues;

using PheasantTails.TwiHigh.Functions.Core.Entity;

public class UpdateUserQueue
{
    public TwiHighUser TwiHighUser { get; set; }

    public UpdateUserQueue(TwiHighUser twiHighUser)
    {
        TwiHighUser = twiHighUser;
    }
}
