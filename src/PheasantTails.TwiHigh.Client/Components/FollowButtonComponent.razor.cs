using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class FollowButtonComponent
    {
        [Parameter]
        public bool IsFollowing { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnClickFollowButton { get; set; }

        //[Parameter]
        //public EventCallback<MouseEventArgs> OnClickUnfollowButton { get; set; }
    }
}
