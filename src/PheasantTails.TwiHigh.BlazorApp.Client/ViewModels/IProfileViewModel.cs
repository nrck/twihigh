using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IProfileViewModel
{
    ReactivePropertySlim<bool> CanExecuteFollowOrRemove { get; }
    AsyncReactiveCommand<DisplayTweet> DeleteMyTweetCommand { get; }
    AsyncReactiveCommand FollowUserDisplayedOnScreenCommand { get; }
    AsyncReactiveCommand<string?> GetUserTimelineCommand { get; }
    ReactivePropertySlim<bool> IsMyTwiHighUser { get; }
    ReactiveCommandSlim NavigateProfileEditorPageCommand { get; }
    ReactiveCommand<ITweet> NavigateStatePageCommand { get; }
    ReactiveCommand<ITwiHighUserSummary> NavigateUserPageCommand { get; }
    ReactivePropertySlim<string> PageTitle { get; }
    AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; }
    AsyncReactiveCommand RemoveUserDisplayedOnScreenCommand { get; }
    ReadOnlyCollection<DisplayTweet>? Tweets { get; }
    ReactivePropertySlim<ResponseTwiHighUserContext?> UserDisplayedOnScreen { get; }
}
