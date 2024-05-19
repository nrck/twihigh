using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
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

    [Inject]
    protected IFeedWorkerService FeedWorkerService { get; set; } = default!;

    [Inject]
    protected IMessageService MessageService { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }

    [CascadingParameter]
    protected HttpContext? HttpContext { get; set; }

    protected async Task InvokeRenderAsync() => await InvokeAsync(StateHasChanged).ConfigureAwait(false);

    protected async void InvokeRender() => await InvokeAsync(StateHasChanged).ConfigureAwait(false);

    protected bool IsServerSideRendering => HttpContext != null;

    public IDisposable SubscribeStateHasChanged<T>(IObservable<T> command) => command.Subscribe(async (_) => await InvokeRenderAsync());

    public IDisposable SubscribeStateHasChanged<T>(AsyncReactiveCommand<T> command) => command.Subscribe(async (_) => await InvokeRenderAsync());

    public void SubscribeStateHasChanged<T>(ReactiveCollection<T> collection) => collection.CollectionChanged += (_, _) => InvokeRender();

    public async ValueTask<Guid> GetLoginUserIdAsync()
    {
        string userId = await ((IAuthenticationStateAccesser)AuthenticationStateProvider).GetLoggedInUserIdAsync();
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

    protected async ValueTask FeedStartAsync()
    {
        string userId = await ((IAuthenticationStateAccesser)AuthenticationStateProvider).GetLoggedInUserIdAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }
        FeedWorkerService.OnChangedFeedTimeline += InvokeRender;
        FeedWorkerService.Run();
    }

    protected ValueTask FeedStopAsync()
    {
        FeedWorkerService.Stop();
        FeedWorkerService.OnChangedFeedTimeline -= InvokeRender;
        return ValueTask.CompletedTask;
    }

    protected void MessageServiceStart()
    {
        MessageService.OnChangedMessage += InvokeRender;
    }

    protected void MessageServiceStop()
    {
        MessageService.OnChangedMessage -= InvokeRender;
    }
}
