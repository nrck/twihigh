using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IFeedsViewModel : IViewModelBase
{
    AsyncReactiveCommand<ITweet> NavigateStatePageCommand { get; }
    AsyncReactiveCommand<ITweet> NavigateStatePageWithReplyCommand { get; }
    ReactiveCommandSlim<string> NavigateUserPageCommand { get; }
}
