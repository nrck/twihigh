using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Bases;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Components;

public partial class THUserIdLink : TwiHighComponentBase
{
    [Parameter]
    public string UserDisplayId { get; set; } = string.Empty;

    private void OnClickLink()
        => base.Navigation.NavigateToProfilePage(UserDisplayId);
}
