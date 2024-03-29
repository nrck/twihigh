﻿namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Logout : PageBase
    {
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOutAsync();
            SetInfoMessage("ログアウトしました。");
            Navigation.NavigateTo(DefinePaths.PAGE_PATH_LOGIN);
        }
    }
}
