﻿namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Index : PageBase
    {
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var isAuthenticated = await GetIsAuthenticatedAsync();
            if (isAuthenticated)
            {
                await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).RefreshAuthenticationStateAsync();
                Navigation.NavigateTo(DefinePaths.PAGE_PATH_HOME, false, true);
                return;
            }

            Navigation.NavigateTo(DefinePaths.PAGE_PATH_LOGIN);
        }
    }
}
