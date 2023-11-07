using Microsoft.JSInterop;

namespace PheasantTails.TwiHigh.Beta.Client.Services
{
    public class ScrollInfoService : IScrollInfoService
    {
        private readonly IJSRuntime _jsRuntime;

        public event EventHandler<string[]>? OnScroll;

        public ScrollInfoService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            RegisterServiceViaJsRuntime();
        }

        [JSInvokable("ScrolledVisbleArticles")]
        public void JsOnScroll(string[] articleIds)
        {
            OnScroll?.Invoke(this, articleIds);
        }

        public ValueTask Enable() => _jsRuntime.InvokeVoidAsync("EnableScrollEventHandling");

        public ValueTask Disable() => _jsRuntime.InvokeVoidAsync("DisableScrollEventHandling");

        private void RegisterServiceViaJsRuntime() => _jsRuntime.InvokeVoidAsync("RegisterScrollInfoService", DotNetObjectReference.Create(this));

    }
}
