using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PheasantTails.TwiHigh.Client.Services;
using static PheasantTails.TwiHigh.Client.Components.MessageComponent;

namespace PheasantTails.TwiHigh.Client.Shared
{
    public abstract class SharedBase : ComponentBase, IDisposable
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        protected IMessageService MessageService { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected IScrollInfoService ScrollInfoService { get; set; }

        [Inject]
        protected IFeedService FeedService { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        [CascadingParameter]
        protected Task<AuthenticationState>? AuthenticationState { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            FeedService.NotifyChangedFeeds += StateHasChanged;
        }

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
            if (AuthenticationState == null)
            {
                return false;
            }
            return (await AuthenticationState).User.Identity?.IsAuthenticated ?? false;
        }

        public virtual void Dispose()
        {
            if (FeedService != null && FeedService.NotifyChangedFeeds != null)
            {
#pragma warning disable CS8601 // Null 参照代入の可能性があります。
                FeedService.NotifyChangedFeeds -= StateHasChanged;
#pragma warning restore CS8601 // Null 参照代入の可能性があります。
            }
            GC.SuppressFinalize(this);
        }
    }
}
