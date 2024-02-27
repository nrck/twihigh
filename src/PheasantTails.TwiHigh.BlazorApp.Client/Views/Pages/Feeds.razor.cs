﻿using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Feeds : TwiHighPageBase
{
    [Inject]
    public IFeedsViewModel ViewModel { get; set; } = default!;

    [Inject]
    public IFeedWorkerService FeedService { get; set; } = default!;

    [Inject]
    public IScrollInfoService ScrollInfoService { get; set; } = default!;

    public override void Dispose()
    {
        FeedService.OnChangedFeedTimeline -= InvokeRender;
        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        FeedService.OnChangedFeedTimeline += InvokeRender;
    }
}