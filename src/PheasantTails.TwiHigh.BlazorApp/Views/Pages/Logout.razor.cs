using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Views.Pages;

public partial class Logout : TwiHighPageBase
{
    [Inject]
    public IMessageService MessageService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await SignOutAsync().ConfigureAwait(false);
        Navigation.NavigateTo("/", forceLoad: true, replace: true);
    }

    private async Task SignOutAsync()
    {
        ArgumentNullException.ThrowIfNull(HttpContext, nameof(HttpContext));
        await HttpContext.SignOutAsync().ConfigureAwait(false);
    }
}
