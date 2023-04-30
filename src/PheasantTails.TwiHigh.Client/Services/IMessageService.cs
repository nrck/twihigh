using System.Collections.ObjectModel;
using static PheasantTails.TwiHigh.Client.Components.MessageComponent;

namespace PheasantTails.TwiHigh.Client.Services
{
    public interface IMessageService
    {
        public Action OnChangedMessage { get; set; }
        public ReadOnlyCollection<MessageContext> Messages { get; }
        public void Set(MessageLevel level, string message);
    }
}
