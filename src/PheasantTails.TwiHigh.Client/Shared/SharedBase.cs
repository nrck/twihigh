using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PheasantTails.TwiHigh.Client.Services;
using static PheasantTails.TwiHigh.Client.Components.MessageComponent;

namespace PheasantTails.TwiHigh.Client.Shared
{
    public abstract class SharedBase : ComponentBase
    {
        [Inject]
        protected IMessageService MessageService { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected IScrollInfoService ScrollInfoService { get; set; }

        [CascadingParameter]
        protected Task<AuthenticationState>? AuthenticationState { get; set; }


        protected void SetErrorMessage(string text)
        {
            MessageService.Set(MessageLevel.Error, text);
        }

        protected void SetWarnMessage(string text)
        {
            MessageService.Set(MessageLevel.Warn, text);
        }

        protected void SetSucessMessage(string text)
        {
            MessageService.Set(MessageLevel.Success, text);
        }

        protected void SetInfoMessage(string text)
        {
            MessageService.Set(MessageLevel.Info, text);
        }

        protected async Task<bool> GetIsAuthenticatedAsync()
        {
            if(AuthenticationState == null)
            {
                return false;
            }
            return (await AuthenticationState).User.Identity?.IsAuthenticated ?? false;
        }
    }
}
