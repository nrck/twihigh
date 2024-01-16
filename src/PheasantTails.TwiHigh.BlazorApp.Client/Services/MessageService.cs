using System.Collections.ObjectModel;
using static PheasantTails.TwiHigh.Client.Components.MessageComponent;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public class MessageService : IMessageService
{
    private readonly List<MessageContext> _messages;

    public ReadOnlyCollection<MessageContext> Messages => new(_messages);

    public Action? OnChangedMessage { get; set; }

    public MessageService()
    {
        _messages = [];
    }

    public void Set(MessageLevel level, string message)
    {
        _messages.Add(new MessageContext(level, message, OnClickClose));
        OnChangedMessage?.Invoke();
    }

    public void SetErrorMessage(string text) => Set(MessageLevel.Error, text);

    public void SetWarnMessage(string text) => Set(MessageLevel.Warn, text);

    public void SetSucessMessage(string text) => Set(MessageLevel.Success, text);

    public void SetInfoMessage(string text) => Set(MessageLevel.Info, text);

    private void OnClickClose(Guid id)
    {
        var context = _messages.FirstOrDefault(context => context.MessageId == id);
        if (context == default)
        {
            return;
        }
        _messages.Remove(context);
        OnChangedMessage?.Invoke();
    }
}
