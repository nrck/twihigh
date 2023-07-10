using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class UpdateUserQueue
    {
        public TwiHighUser TwiHighUser { get; set; }

        public UpdateUserQueue(TwiHighUser twiHighUser) 
        {
            TwiHighUser = twiHighUser;
        }

        public static implicit operator UpdateUserQueue(TwiHighUser twiHighUser) => new(twiHighUser);
    }
}
