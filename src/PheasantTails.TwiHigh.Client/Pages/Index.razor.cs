using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Index
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private NavigationManager Navigation { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (AuthenticationState != null)
            {
                var isAuthenticated = (await AuthenticationState).User.Identity?.IsAuthenticated ?? false;
                if (isAuthenticated)
                {
                    Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME, false, true);
                    return;
                }
            }

            await base.OnInitializedAsync();
            Navigation.NavigateTo(DefinePaths.PAGE_PATH_LOGIN);
        }
    }
}
