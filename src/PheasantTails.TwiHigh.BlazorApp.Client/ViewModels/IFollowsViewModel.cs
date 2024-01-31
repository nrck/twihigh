using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IFollowsViewModel : IViewModelBase
{
    ReactivePropertySlim<bool> CanExequteGetTwiHighUserFollowsCommand { get; }
    AsyncReactiveCommand<string> GetTwiHighUserFollowsCommand { get; }
    ReactivePropertySlim<string> PageTitle { get; }
    ReactivePropertySlim<ResponseTwiHighUserContext?> UserDisplayedOnScreen { get; }
    ReactiveCollection<ResponseTwiHighUserContext> UserFollows { get; }
}
