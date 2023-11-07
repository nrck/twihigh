using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PheasantTails.TwiHigh.Beta.Client.Components
{
    public partial class FollowButtonComponent
    {
        [Parameter]
        public bool IsFollowing { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnClickFollowButton { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnClickRemoveButton { get; set; }

        private string FollowedButtonText { get; set; } = "フォロー中";

        private void OnMouseOverRemoveButton(MouseEventArgs e)
        {
            FollowedButtonText = "リムーブする";
        }

        private void OnMouseOutRemoveButton(MouseEventArgs e)
        {
            FollowedButtonText = "フォロー中";
        }
    }
}
