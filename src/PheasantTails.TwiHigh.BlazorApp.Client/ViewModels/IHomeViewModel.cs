using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IHomeViewModel : IViewModelBase
{
    ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId Sticker)> AddReactionCommand { get; }
    ReactiveCommand<ICosmosDbItemId> AddRetweetCommand { get; }
    AsyncReactiveCommand<ICosmosDbItemId> DeleteMyTweetCommand { get; }
    ReactiveCommand<(ICosmosDbItemId Tweet, ITwiHighUserSummary User)> NavigateStatePageCommand { get; }
    ReactiveCommand<ICosmosDbItemId> NavigateStatePageWithReplyCommand { get; }
    ReactiveCommand<ICosmosDbItemId> NavigateUserPageCommand { get; }
    ReactiveCommand<(ICosmosDbItemId Tweet, ICosmosDbItemId User)> RemoveReactionCommand { get; }
    ReactiveCommand<ICosmosDbItemId> RemoveRetweetCommand { get; }
    ReactivePropertySlim<string> AvatarUrl { get; }
    ReactiveCommand NavigateProfileEditorPageCommand { get; }
    AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; }
}