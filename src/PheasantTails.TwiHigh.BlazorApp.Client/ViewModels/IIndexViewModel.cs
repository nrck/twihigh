using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public interface IIndexViewModel : IViewModelBase
{
    AsyncReactiveCommand CheckAuthenticationStateCommand { get; }
}