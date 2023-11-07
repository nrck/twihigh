using Microsoft.AspNetCore.Components;

namespace PheasantTails.TwiHigh.Beta.Client.Components
{
    public partial class BigAvatarComponent
    {
        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;
    }
}
