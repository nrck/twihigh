using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Login
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private AppUserHttpClient AppUserHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        private PostAuthorizationContext PostAuthorizationContext { get; set; } = new PostAuthorizationContext();

        private string ErrorMessage { get; set; } = string.Empty;

        private bool IsLoginWorking { get; set; } = false;

        private async Task OnClickLoginButtonAsync(MouseEventArgs _)
        {
            ErrorMessage = string.Empty;
            if (IsLoginWorking == true)
            {
                return;
            }

            IsLoginWorking = true;
            if (string.IsNullOrEmpty(PostAuthorizationContext.DisplayId))
            {
                ErrorMessage = "ユーザ名を入力してください。";
                IsLoginWorking = false;
                return;
            }
            if (string.IsNullOrEmpty(PostAuthorizationContext.PlanePassword))
            {
                ErrorMessage = "パスワードを入力してください。";
                IsLoginWorking = false;
                return;
            }

            var res = await AppUserHttpClient.LoginAsync(PostAuthorizationContext);
            if (string.IsNullOrEmpty(res?.Token))
            {
                ErrorMessage = "ログインできませんでした。ユーザ名とパスワードを確認してください。";
            }
            IsLoginWorking = false;
            return;
        }
    }
}
