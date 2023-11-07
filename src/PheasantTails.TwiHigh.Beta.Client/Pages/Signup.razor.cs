using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Beta.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Beta.Client.Pages
{
    public partial class Signup : PageBase
    {
        private AddTwiHighUserContext Context { get; set; } = new AddTwiHighUserContext();

        private AddTwiHighUserContextValidator Validator { get; } = new AddTwiHighUserContextValidator();

        private bool IsWorking { get; set; } = false;

        private async Task OnClickSignupButtonAsync(MouseEventArgs _)
        {
            if (IsWorking)
            {
                return;
            }
            IsWorking = true;

            Context.DisplayName = Context.DisplayId;

            // バリデーションの実施
            var result = Validator.Validate(Context);
            if (!result.IsValid)
            {
                SetErrorMessage(result.Errors.FirstOrDefault()?.ErrorMessage ?? "入力項目を見直してください。");
                IsWorking = false;
                return;
            }
            var response = await AppUserHttpClient.SignUpAsync(Context);
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                SetErrorMessage($"すでに @{Context.DisplayId} は使用されているようです。他のアカウントIDを使用してください。");
                IsWorking = false;
                return;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                SetErrorMessage($"アカウントIDは必ず入力する必要があります。");
                IsWorking = false;
                return;
            }
            else if (!response.IsSuccessStatusCode)
            {
                SetErrorMessage($"申し訳ありません。サーバーでエラーが発生しました。({response.StatusCode})");
                IsWorking = false;
                return;
            }

            var loginContext = new PostAuthorizationContext
            {
                DisplayId = Context.DisplayId,
                PlanePassword = Context.Password
            };

            var loginResponse = await AppUserHttpClient.LoginAsync(loginContext);
            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                SetErrorMessage($"ログインに失敗しました。");
                IsWorking = false;
                return;
            }

            await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticatedAsync(loginResponse.Token);
            IsWorking = false;
            Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME);
        }
    }
}
