using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components;

public partial class THFollowButton : TwiHighComponentBase
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
