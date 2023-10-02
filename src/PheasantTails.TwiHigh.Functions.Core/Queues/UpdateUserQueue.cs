using PheasantTails.TwiHigh.Interface;

namespace PheasantTails.TwiHigh.Functions.Core.Queues;

public class UpdateUserQueue
{
    public ITwiHighUser TwiHighUser { get; set; }

    public UpdateUserQueue(ITwiHighUser twiHighUser)
    {
        TwiHighUser = twiHighUser;
    }
}
