using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Beta.Client.Pages;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Beta.Client.Components
{
    public partial class TwiHighUserComponent
    {
        [Parameter]
        public ResponseTwiHighUserContext? User { get; set; }

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private NavigationManager Navigation { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        private void OnClickAvatar(MouseEventArgs _) => NavigateToProfilePage();

        private void OnClickUserDisplayName(MouseEventArgs _) => NavigateToProfilePage();

        private void OnClickUserDisplayId(MouseEventArgs _) => NavigateToProfilePage();

        private void NavigateToProfilePage()
        {
            if (User == null)
            {
                return;
            }

            var url = string.Format(DefinePaths.PAGE_PATH_PROFILE, User.DisplayId);
            Navigation.NavigateTo(url);
        }
    }
}
