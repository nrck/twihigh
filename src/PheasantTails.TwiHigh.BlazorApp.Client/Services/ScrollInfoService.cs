using Microsoft.JSInterop;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public class ScrollInfoService : IScrollInfoService
{
    private readonly IJSRuntime _jsRuntime;

    /// <inheritdoc/>
    public event Action<string[]>? OnScroll;

    public ScrollInfoService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        RegisterServiceViaJsRuntime();
    }

    /// <summary>
    /// Invokable method from JS.
    /// </summary>
    [JSInvokable("ScrolledVisbleArticles")]
    public void JsOnScroll(string[] articleIds) => OnScroll?.Invoke(articleIds);

    /// <inheritdoc/>
    public ValueTask Enable() => _jsRuntime.InvokeVoidAsync("EnableScrollEventHandling");

    /// <inheritdoc/>
    public ValueTask Disable() => _jsRuntime.InvokeVoidAsync("DisableScrollEventHandling");

    /// <summary>
    /// Register service via JS.
    /// </summary>
    private async void RegisterServiceViaJsRuntime() => await _jsRuntime.InvokeVoidAsync("RegisterScrollInfoService", DotNetObjectReference.Create(this)).ConfigureAwait(false);
}
