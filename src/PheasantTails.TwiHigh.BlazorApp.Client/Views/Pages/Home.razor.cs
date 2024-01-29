using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Home : TwiHighPageBase
{
    [Inject]
    public IHomeViewModel ViewModel { get; set; } = default!;

    [Inject]
    public ITimelineWorkerService WorkerService { get; set; } = default!;

    [Inject]
    public IScrollInfoService ScrollInfoService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync().ConfigureAwait(false);
        ScrollInfoService.OnScroll += ViewModel.MarkAsReadedTweetCommand.Execute;
        WorkerService.OnChangedTimeline += InvokeRender;
        await ViewModel.GetLoginUserIdCommand.ExecuteAsync().ConfigureAwait(false);
        await ViewModel.GetMyAvatarUrlCommand.ExecuteAsync().ConfigureAwait(false);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);
        if (firstRender)
        {
            await ScrollInfoService.EnableAsync();
            WorkerService.Run();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        ScrollInfoService.OnScroll -= ViewModel.MarkAsReadedTweetCommand.Execute;
        WorkerService.OnChangedTimeline -= InvokeRender;
        WorkerService.Stop();
        await ScrollInfoService.DisableAsync();
        GC.SuppressFinalize(this);
    }
}
