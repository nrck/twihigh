using Microsoft.JSInterop;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public class ScrollInfoService(IJSRuntime jsRuntime) : IScrollInfoService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));

    /// <inheritdoc/>
    public event Action<string[]>? OnScroll;

    /// <summary>
    /// Invokable method from JS.
    /// </summary>
    [JSInvokable("ScrolledVisbleArticles")]
    public void JsOnScroll(string[] articleIds) => OnScroll?.Invoke(articleIds);

    /// <inheritdoc/>
    public async ValueTask EnableAsync()
    {
        await RegisterServiceViaJsRuntime().ConfigureAwait(false);
        await _jsRuntime.InvokeVoidAsync("EnableScrollEventHandling").ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public ValueTask DisableAsync() => _jsRuntime.InvokeVoidAsync("DisableScrollEventHandling");

    /// <summary>
    /// Register service via JS.
    /// </summary>
    private ValueTask RegisterServiceViaJsRuntime() => _jsRuntime.InvokeVoidAsync("RegisterScrollInfoService", DotNetObjectReference.Create(this));
}
