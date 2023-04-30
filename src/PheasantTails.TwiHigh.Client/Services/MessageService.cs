using System.Collections.Generic;
using System.Collections.ObjectModel;
using static PheasantTails.TwiHigh.Client.Components.MessageComponent;

namespace PheasantTails.TwiHigh.Client.Services
{
    public class MessageService : IMessageService
    {
        private readonly List<MessageContext> _messages;

        public ReadOnlyCollection<MessageContext> Messages => new ReadOnlyCollection<MessageContext>(_messages);

        public Action OnChangedMessage { get; set; }

        public MessageService()
        {
            _messages = new List<MessageContext>();
        }

        public void Set(MessageLevel level, string message)
        {
            _messages.Add(new MessageContext(level, message, OnClickClose));
            OnChangedMessage.Invoke();
        }

        private void OnClickClose(Guid id)
        {
            var context = _messages.FirstOrDefault(context => context.MessageId == id);
            if(context == default)
            {
                return;
            }
            _messages.Remove(context);
            OnChangedMessage.Invoke();
        }
    }
}
