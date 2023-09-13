using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.Feeds;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Feeds : PageBase
    {
        private FeedContext[]? MyFeeds { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var res = await FeedHttpClient.GetMyFeedsAsync(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            if (res == null)
            {
                MyFeeds = Array.Empty<FeedContext>();
            }
            else
            {
                MyFeeds = res.Feeds;
            }
            StateHasChanged();
        }
    }
}
