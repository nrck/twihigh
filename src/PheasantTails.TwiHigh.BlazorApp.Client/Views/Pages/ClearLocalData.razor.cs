using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Exceptions;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

[Authorize]
public partial class ClearLocalData : TwiHighPageBase
{
    [Inject]
    public ITimelineWorkerService TimelineWorkerService { get; set; } = default!;

    [Inject]
    public IFeedWorkerService FeedWorkerService { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Clear local storage.
                await TimelineWorkerService.CacheClearAsync();
                await FeedWorkerService.CacheClearAsync();
            }
            catch (TwiHighException)
            {
            }

            // Navigate to home page.
            Navigation.NavigateToHomePage(replace: true);
        }
    }
}
