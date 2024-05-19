namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

public class TwiHighPageBase : TwiHighUIBase
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync().ConfigureAwait(false);
        MessageServiceStart();
        await FeedStartAsync().ConfigureAwait(false);
    }

    public override async ValueTask DisposeAsync()
    {
        await FeedStopAsync().ConfigureAwait(false);
        MessageServiceStop();
        await base.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
