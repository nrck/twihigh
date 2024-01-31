using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IFollowersViewModel : IViewModelBase
{
    ReactivePropertySlim<bool> CanExequteGetTwiHighUserFollowersCommand { get; }
    AsyncReactiveCommand<string> GetTwiHighUserFollowersCommand { get; }
    ReactivePropertySlim<string> PageTitle { get; }
    ReactivePropertySlim<ResponseTwiHighUserContext?> UserDisplayedOnScreen { get; }
    ReactiveCollection<ResponseTwiHighUserContext> UserFollowers { get; }
}
