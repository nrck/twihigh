using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components;

public partial class THMessage : TwiHighComponentBase
{
    public enum MessageLevel
    {
        Info,
        Success,
        Warn,
        Error
    }

    public class MessageContext(MessageLevel level, string message, Action<Guid> onClickClose)
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public string Message { get; set; } = message;
        public MessageLevel Level { get; set; } = level;
        public Action<Guid> OnClickClose { get; set; } = onClickClose;
    }

    [Parameter]
    public MessageContext? Context { get; set; }

    private string MessageAreaCssClass => Context?.Level switch
    {
        MessageLevel.Success => "success",
        MessageLevel.Error => "error",
        MessageLevel.Warn => "warn",
        _ => "info",
    };

    private bool IsClosed { get; set; }

    private Timer AutoCloseTimer { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        AutoCloseTimer = new Timer(OnFireAutoCloseTimer, null, 5000, 0);
    }

    private void OnClickCloseButton(MouseEventArgs e) => CloseMessage();

    private void OnFireAutoCloseTimer(object? state) => CloseMessage();

    private void CloseMessage()
    {
        if (Context == null || IsClosed == true)
        {
            return;
        }
        IsClosed = true;
        AutoCloseTimer.Dispose();
        InvokeRender();
        _ = new Timer((_) => { Context.OnClickClose.Invoke(Context.MessageId); }, null, 500, 0);
        ;
    }
}
