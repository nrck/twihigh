using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Feeds : TwiHighPageBase
{
    [Inject]
    public IFeedsViewModel ViewModel { get; set; } = default!;

    [Inject]
    public IScrollInfoService ScrollInfoService { get; set; } = default!;
}
