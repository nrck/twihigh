using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Pages;

public partial class Logout : TwiHighPageBase
{
    [Inject]
    public IMessageService MessageService { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnInitializedAsync();
        if (firstRender)
        {
            //((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOut();
            MessageService.SetInfoMessage("ログアウトしました。");
            Navigation.NavigateToLoginPage(replace: true);
        }
    }
}
