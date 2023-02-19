using Microsoft.AspNetCore.Components;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class AvatarComponent
    {
        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;
    }
}
