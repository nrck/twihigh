using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;
{
    public partial class Feeds : TwiHighPageBase
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
            ScrollInfoService.OnScroll += MarkAsOpenedFeeds;
        }

        protected override void Dispose(bool isDispose)
        {
            if (isDispose)
            {
                if (FeedService?.NotifyChangedFeeds != null)
                {
                    FeedService.NotifyChangedFeeds -= UpdateMyFeeds;
                }
                ScrollInfoService.OnScroll -= MarkAsOpenedFeeds;
                ViewModel.Dispose();
            }
            base.Dispose(isDispose);
        }

        private void UpdateMyFeeds()
        {
            ViewModel.MyFeeds = FeedService.FeedContexts;
        }

        private async void MarkAsOpenedFeeds(object? sender, string[] ids)
        {
            if (ViewModel?.MyFeeds == null)
            {
                return;
            }
            List<Guid> feedIds = new();
            foreach (var id in ids)
            {
                if (Guid.TryParse(id[6..], out var guid))
                {
                    feedIds.Add(guid);
                }
            }
            await FeedService.MarkAsReadedFeedAsync(feedIds);
        }
    }
}
