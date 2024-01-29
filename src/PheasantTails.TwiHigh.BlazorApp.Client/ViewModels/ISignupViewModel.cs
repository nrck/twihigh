using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface ISignupViewModel : IViewModelBase
{
    ReactivePropertySlim<bool> CanSignupCommand { get; }
    ReactivePropertySlim<string> DisplayId { get; }
    ReactivePropertySlim<string> Email { get; }
    ReactivePropertySlim<string> Password { get; }
    AsyncReactiveCommand SignupCommand { get; }
}
