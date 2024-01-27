using PheasantTails.TwiHigh.Data.Model.Feeds;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels
{
    internal interface IFeedsViewModel
    {
        ReactiveCollection<FeedContext> MyFeeds { get; }
        AsyncReactiveCommand<ITweet> NavigateStatePageCommand { get; }
        AsyncReactiveCommand<ITweet> NavigateStatePageWithReplyCommand { get; }
        ReactiveCommandSlim<string> NavigateUserPageCommand { get; }
    }
}