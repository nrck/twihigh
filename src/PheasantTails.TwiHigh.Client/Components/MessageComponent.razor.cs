using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class MessageComponent : IAsyncDisposable
    {
        public enum MessageLevel
        {
            Info,
            Success,
            Warn,
            Error
        }

        [Parameter]
        public Guid MessageId { get; set; }

        [Parameter]
        public string Message { get; set; } = string.Empty;

        [Parameter]
        public MessageLevel Level { get; set; } = MessageLevel.Info;

        [Parameter]
        public EventCallback<Guid> OnClickClose { get; set; }

        private string MessageAreaCssClass => Level switch
        {
            MessageLevel.Success => "success",
            MessageLevel.Error => "error",
            MessageLevel.Warn => "warn",
            _ => "info",
        };

        private bool IsDispose { get; set; }

        private void OnClickCloseButton(MouseEventArgs e)
        {
            OnClickClose.InvokeAsync(MessageId);
        }

        public ValueTask DisposeAsync()
        {
            if (IsDispose)
            {
                return ValueTask.CompletedTask;
            }
            IsDispose = true;
            StateHasChanged();
            var task = Task.Run(async () => {
                await Task.Delay(1000);
                GC.SuppressFinalize(this);
            });
            return new ValueTask(task);
        }
    }
}
