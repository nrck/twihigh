using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IStatusViewModel : IViewModelBase
{
    ReactivePropertySlim<bool> CanScroll { get; }
    AsyncReactiveCommand<DisplayTweet> DeleteMyTweetCommand { get; }
    AsyncReactiveCommand<string> FetchTweetsCommand { get; }
    ReactiveCommandSlim NavigateProfileEditorPageCommand { get; }
    ReactiveCommandSlim<DisplayTweet> NavigateStatePageCommand { get; }
    ReactiveCommandSlim<ITwiHighUserSummary> NavigateUserPageCommand { get; }
    ReactivePropertySlim<string> PageTitle { get; }
    AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; }
    AsyncReactiveCommand<string> ScrollToTargetTweetCommand { get; }
    ReadOnlyCollection<DisplayTweet>? Tweets { get; }
}
