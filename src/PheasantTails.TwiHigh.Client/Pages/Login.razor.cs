using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Login : PageBase
    {
        private PostAuthorizationContext PostAuthorizationContext { get; set; } = new PostAuthorizationContext();

        private bool IsLoginWorking { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (AuthenticationState != null)
            {
                var isAuthenticated = (await AuthenticationState).User.Identity?.IsAuthenticated ?? false;
                if (isAuthenticated)
                {
                    Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME);
                    return;
                }
            }
        }

        private async Task OnClickLoginButtonAsync(MouseEventArgs _)
        {
            if (IsLoginWorking == true)
            {
                return;
            }

            IsLoginWorking = true;
            if (string.IsNullOrEmpty(PostAuthorizationContext.DisplayId))
            {
                SetErrorMessage("ユーザ名を入力してください。");
                IsLoginWorking = false;
                return;
            }
            if (string.IsNullOrEmpty(PostAuthorizationContext.PlanePassword))
            {
                SetErrorMessage("パスワードを入力してください。");
                IsLoginWorking = false;
                return;
            }

            var res = await AppUserHttpClient.LoginAsync(PostAuthorizationContext);
            if (string.IsNullOrEmpty(res?.Token))
            {
                SetErrorMessage("ログインできませんでした。ユーザ名とパスワードを確認してください。");
                IsLoginWorking = false;
                return;
            }
            IsLoginWorking = false;
            await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticatedAsync(res?.Token ?? string.Empty);
            SetInfoMessage("ログインしました。");
            Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME, false, true);
            return;
        }
    }
}
