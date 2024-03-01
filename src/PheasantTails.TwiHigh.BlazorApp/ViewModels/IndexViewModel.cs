using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

namespace PheasantTails.TwiHigh.BlazorApp.ViewModels;

public class IndexViewModel : Client.ViewModels.IndexViewModel, IIndexViewModel
{
    public IndexViewModel(AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager, IMessageService messageService)
        : base(authenticationStateProvider, navigationManager, messageService)
    {
    }

    protected override void Subscribe()
    {
        // On Server, Invoke at OnInitializedAsync method.
        CheckAuthenticationStateOnAfterRenderCommand.Subscribe(LoginCheck);
    }
}
