using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components;

public partial class THUser : TwiHighPageBase
{
    [Parameter, EditorRequired]
    public ResponseTwiHighUserContext? User { get; set; }

    private void OnClickAvatar(MouseEventArgs _) => NavigateToProfilePage();

    private void OnClickUserDisplayName(MouseEventArgs _) => NavigateToProfilePage();

    private void OnClickUserDisplayId(MouseEventArgs _) => NavigateToProfilePage();

    private void NavigateToProfilePage()
    {
        if (User == null)
        {
            return;
        }

        Navigation.NavigateToProfilePage(User.DisplayId);
    }
}
