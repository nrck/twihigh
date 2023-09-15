using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Client.ViewModels;
using PheasantTails.TwiHigh.Data.Model.Feeds;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Feeds : PageBase
    {
        private FeedContext[]? MyFeeds { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            MyFeeds = FeedService.FeedContexts;
            StateHasChanged();
        }

        private void OnClickDetail(TweetViewModel tweetViewModel) => 
            Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_STATUS, tweetViewModel.UserDisplayId, tweetViewModel.Id));

        private void OnClickProfile(TweetViewModel tweetViewModel) =>
            Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_PROFILE, tweetViewModel.UserDisplayId));
    }
}
