using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IHomeViewModel : IViewModelBase
{
    ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)> AddReactionCommand { get; }
    ReactiveCommand<ICosmosDbItemId> AddRetweetCommand { get; }
    AsyncReactiveCommand<ICosmosDbItemId> DeleteMyTweetCommand { get; }
    ReactiveCommand<ITweet> NavigateStatePageCommand { get; }
    ReactiveCommand<ITweet> NavigateStatePageWithReplyCommand { get; }
    ReactiveCommand<ITwiHighUserSummary> NavigateUserPageCommand { get; }
    ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)> RemoveReactionCommand { get; }
    ReactiveCommand<ICosmosDbItemId> RemoveRetweetCommand { get; }
    ReactivePropertySlim<string> AvatarUrl { get; }
    ReactiveCommand NavigateProfileEditorPageCommand { get; }
    AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; }
    ReactivePropertySlim<Guid> MyTwiHighUserId { get; }
    AsyncReactiveCommand GetLoginUserIdCommand { get; }
    AsyncReactiveCommand<DisplayTweet> GetGapTweetCommand { get; }
    AsyncReactiveCommand GetMyAvatarUrlCommand { get; }
    ReactiveCommandSlim<IEnumerable<string>> MarkAsReadedTweetCommand { get; }
    ReactivePropertySlim<bool> IsProcessingMarkAsReaded { get; }
}