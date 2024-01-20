using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Bases;

public class TwiHighUIBase : ComponentBase
{
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    public async Task InvokeRenderAsync() => await InvokeAsync(StateHasChanged);
}
