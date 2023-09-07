using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Feeds : PageBase
    {
        private Feed[]? MyFeeds { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var res = await FeedHttpClient.GetMyFeedsAsync(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            if(res != null)
            {
                MyFeeds = res.Feeds;
                StateHasChanged();
            }
        }
    }
}
