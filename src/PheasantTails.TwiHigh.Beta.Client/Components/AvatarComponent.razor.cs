using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;

namespace PheasantTails.TwiHigh.Beta.Client.Components
{
    public partial class AvatarComponent
    {
        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<MouseEventArgs> OnClick { get; set; }
    }
}
