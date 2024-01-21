using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class IndexViewModel : ViewModelBase, IIndexViewModel
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AsyncReactiveCommand CheckAuthenticationStateOnAfterRenderCommand { get; } = new AsyncReactiveCommand();

    public AsyncReactiveCommand CheckAuthenticationStateOnInitializedCommand { get; } = new AsyncReactiveCommand();

    public IndexViewModel(AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager, IMessageService messageService) : base(navigationManager, messageService)
    {
        _authenticationStateProvider = authenticationStateProvider;
        CheckAuthenticationStateOnAfterRenderCommand.AddTo(_disposable);
        CheckAuthenticationStateOnInitializedCommand.AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        // On WebAssembly, Invoke at OnInitializedAsync method.
        CheckAuthenticationStateOnInitializedCommand.Subscribe(LoginCheck);
    }

    protected async Task LoginCheck()
    {
        var state = await ((TwiHighAuthenticationStateProvider)_authenticationStateProvider).GetAuthenticationStateAsync().ConfigureAwait(false);
        var isAuthenticated = state.User.Identity?.IsAuthenticated ?? false;
        if (isAuthenticated)
        {
            _navigationManager.NavigateToHomePage(false, true);
        }
        else
        {
            _navigationManager.NavigateToLoginPage();
        }
    }
}
