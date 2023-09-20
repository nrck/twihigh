using PheasantTails.TwiHigh.Client.ViewModels;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Feeds : PageBase
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        private FeedsViewModel ViewModel { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ViewModel = new FeedsViewModel(this)
            {
                MyFeeds = FeedService.FeedContexts
            };
            FeedService.NotifyChangedFeeds += UpdateMyFeeds;
        }

        protected override void Dispose(bool isDispose)
        {
            if (isDispose)
            {
                if (FeedService?.NotifyChangedFeeds != null)
                {
                    FeedService.NotifyChangedFeeds -= UpdateMyFeeds;
                }
                ViewModel.Dispose();
            }
            base.Dispose(isDispose);
        }

        private void UpdateMyFeeds()
        {
            ViewModel.MyFeeds = FeedService.FeedContexts;
        }

        private void OnClickDetail(TweetViewModel tweetViewModel) =>
            Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_STATUS, tweetViewModel.UserDisplayId, tweetViewModel.Id));

        private void OnClickProfile(TweetViewModel tweetViewModel) =>
            Navigation.NavigateTo(string.Format(DefinePaths.PAGE_PATH_PROFILE, tweetViewModel.UserDisplayId));
    }
}
