using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components
{
    public partial class THAvatar : TwiHighComponentBase
    {
        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<MouseEventArgs> OnClick { get; set; }
    }
}
