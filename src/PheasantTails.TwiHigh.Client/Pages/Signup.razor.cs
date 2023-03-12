using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Signup
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private NavigationManager NavigationManager { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private AppUserHttpClient AppUserHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        private AddTwiHighUserContext Context { get; set; } = new AddTwiHighUserContext();

        private AddTwiHighUserContextValidator Validator { get; } = new AddTwiHighUserContextValidator();

        private bool IsWorking { get; set; } = false;

        private string ErrorMessage { get; set; } = string.Empty;


        private async Task OnClickSignupButtonAsync(MouseEventArgs _)
        {
            if (IsWorking)
            {
                return;
            }

            ErrorMessage = string.Empty;
            IsWorking = true;

            // 暫定的にIDをメール名で取る
            Context.DisplayId = Context.Email.Split('@').FirstOrDefault() ?? string.Empty;

            // バリデーションの実施
            var result = Validator.Validate(Context);
            if (!result.IsValid)
            {
                ErrorMessage = result.Errors.FirstOrDefault()?.ErrorMessage ?? "入力項目を見直してください。";
                IsWorking = false;
                return;
            }
            var response = await AppUserHttpClient.SignUpAsync(Context);

            if(string.IsNullOrEmpty(response?.Token))
            {
                ErrorMessage = "ペンギンが参加の邪魔をしたようです！";
                IsWorking = false;
                return;
            }

            await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticatedAsync(response.Token);
            IsWorking = false;
            NavigationManager.NavigateTo(DefinePaths.PAGE_PATH_HOME);
        }
    }
}
