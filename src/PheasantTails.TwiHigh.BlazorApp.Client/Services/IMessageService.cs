using System.Collections.ObjectModel;
using static PheasantTails.TwiHigh.BlazorApp.Client.Views.Components.MessageComponent;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public interface IMessageService
{
    public Action? OnChangedMessage { get; set; }
    public ReadOnlyCollection<MessageContext> Messages { get; }
    public void Set(MessageLevel level, string message);
    public void SetErrorMessage(string text);
    public void SetInfoMessage(string text);
    public void SetSucessMessage(string text);
    public void SetWarnMessage(string text);
}
