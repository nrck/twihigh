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
    public ITimelineWorkerService TimelineWorkerService { get; set; } = default!;

    [Inject]
    public IScrollInfoService ScrollInfoService { get; set; } = default!;

    public override async ValueTask DisposeAsync()
    {
        TimelineStop();
        ScrollInfoService.OnScroll -= ViewModel.MarkAsReadedTweetCommand.Execute;
        await ScrollInfoService.DisableAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync().ConfigureAwait(false);
        TimelineStart();
        ScrollInfoService.OnScroll += ViewModel.MarkAsReadedTweetCommand.Execute;
        await ViewModel.GetLoginUserIdCommand.ExecuteAsync().ConfigureAwait(false);
        await ViewModel.GetMyAvatarUrlCommand.ExecuteAsync().ConfigureAwait(false);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);
        if (firstRender)
        {
            await ScrollInfoService.EnableAsync().ConfigureAwait(false);
        }
    }

    private void TimelineStart()
    {
        TimelineWorkerService.OnChangedTimeline += InvokeRender;
        TimelineWorkerService.Run();
    }

    private void TimelineStop()
    {
        TimelineWorkerService.OnChangedTimeline -= InvokeRender;
        TimelineWorkerService.Stop();
    }
}
