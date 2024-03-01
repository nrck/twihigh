using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components;

public partial class THBigAvatar : TwiHighComponentBase
{
    [Parameter]
    public string UserAvatarUrl { get; set; } = string.Empty;
}
