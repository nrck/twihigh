using Microsoft.AspNetCore.Components;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class BigAvatarComponent
    {
        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;
    }
}
