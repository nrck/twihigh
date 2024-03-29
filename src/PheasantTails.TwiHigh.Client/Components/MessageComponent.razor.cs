﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class MessageComponent : IDisposable
    {
        public enum MessageLevel
        {
            Info,
            Success,
            Warn,
            Error
        }

        public class MessageContext
        {
            public Guid MessageId { get; set; }
            public string Message { get; set; }
            public MessageLevel Level { get; set; }
            public Action<Guid> OnClickClose { get; set; }

            public MessageContext(MessageLevel level, string message, Action<Guid> onClickClose)
            {
                MessageId = Guid.NewGuid();
                Message = message;
                Level = level;
                OnClickClose = onClickClose;
            }
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

        private Timer AutoCloseTimer { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            AutoCloseTimer = new Timer(OnFireAutoCloseTimer,null, 5000, 0);
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
            StateHasChanged();
            _ = new Timer((object? _) => { Context.OnClickClose.Invoke(Context.MessageId); }, null, 500, 0);
            ;
        }
    }
}
