using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

public class TwiHighUIBase : ComponentBase, IDisposable, IAsyncDisposable
{
    protected const string BRAND_NAME = "ツイハイ！";

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }

    protected async Task InvokeRenderAsync() => await InvokeAsync(StateHasChanged).ConfigureAwait(false);

    protected async void InvokeRender() => await InvokeAsync(StateHasChanged).ConfigureAwait(false);

    public IDisposable SubscribeStateHasChanged<T>(IObservable<T> command) => command.Subscribe(async (_) => await InvokeRenderAsync());

    public IDisposable SubscribeStateHasChanged<T>(AsyncReactiveCommand<T> command) => command.Subscribe(async (_) => await InvokeRenderAsync());

    public async ValueTask<Guid> GetLoginUserIdAsync()
    {
        string userId = await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).GetLoggedInUserIdAsync();
        return Guid.Parse(userId);
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
