using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components;

public partial class THUserIdLink : TwiHighComponentBase
{
    [Parameter]
    public string UserDisplayId { get; set; } = string.Empty;

    private void OnClickLink()
        => Navigation.NavigateToProfilePage(UserDisplayId);
}
