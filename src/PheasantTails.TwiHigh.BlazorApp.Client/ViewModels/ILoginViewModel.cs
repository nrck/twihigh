using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface ILoginViewModel : IViewModelBase
{
    ReactivePropertySlim<bool> CanExecute { get; }
    AsyncReactiveCommand CheckAuthenticationStateCommand { get; }
    ReactivePropertySlim<string> DisplayId { get; }
    AsyncReactiveCommand LoginCommand { get; }
    ReactivePropertySlim<string> PlainPassword { get; }
}