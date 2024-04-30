using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Status : TwiHighPageBase
{
    [Inject]
    public IStatusViewModel ViewModel { get; set; } = default!;

    [Parameter]
    public string UserDisplayId { get; set; } = default!;

    [Parameter]
    public string TweetId { get; set; } = default!;

    private Guid MyTwiHithUserId { get; set; }

    public override void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        string id = await ((IAuthenticationStateAccesser)AuthenticationStateProvider).GetLoggedInUserIdAsync().ConfigureAwait(false);
        if (Guid.TryParse(id, out Guid result))
        {
            MyTwiHithUserId = result;
        }
        Navigation.LocationChanged += OnLocationChanged;
        SubscribeStateHasChanged(ViewModel.FetchTweetsCommand);
        SubscribeStateHasChanged(ViewModel.PostTweetCommand);
        SubscribeStateHasChanged(ViewModel.DeleteMyTweetCommand);
        SubscribeStateHasChanged(ViewModel.PageTitle);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync().ConfigureAwait(false);
        await ViewModel.FetchTweetsCommand.ExecuteAsync(TweetId).ConfigureAwait(false);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        await ViewModel.ScrollToTargetTweetCommand.ExecuteAsync(TweetId);
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => await ViewModel.ScrollToTargetTweetCommand.ExecuteAsync(TweetId);
}
