using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Views.Pages;

/// <summary>
/// Login page. This page is static server side rendering(Static SSR).
/// </summary>
public partial class Login : TwiHighPageBase
{
    /// <summary>
    /// Login form model for EditContext.
    /// </summary>
    private class LoginFormModel
    {
        /// <summary>
        /// User`s display id.<br />
        /// ex) `nr_ck`
        /// </summary>
        [Required(ErrorMessage = "アカウントIDを入力してください。")]
        public string DisplayId { get; set; } = string.Empty;

        /// <summary>
        /// User`s login password.
        /// </summary>
        [Required(ErrorMessage = "パスワードを入力してください。")]
        public string PlainPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// On post, this variable set from the login form.
    /// </summary>
    [SupplyParameterFromForm]
    private LoginFormModel Model { get; set; } = default!;

    /// <summary>
    /// The EditContext for login form.
    /// </summary>
    private EditContext EditContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        // If get method, set default value because Model is null.
        Model ??= new LoginFormModel();
        EditContext = new EditContext(Model);

        if (HttpContext?.User.Identity?.IsAuthenticated == true)
        {
            // If was authenticated then navigate to home page.
            Navigation.NavigateTo("/home", forceLoad: false, replace: true);
            return;
        }

        if (Navigation.Uri.Contains('?'))
        {
            // If uri has query string then remove that.
            Navigation.NavigateTo("/login", forceLoad: false, replace: true);
        }
    }

    /// <summary>
    /// On submit handler method.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleValidSubmitAsync(EditContext editContext)
    {
        // If HttpContext is null then throw exception. But on Static SSR, this exception is not throw because HttpContext is set by FW.
        // TODO: use ArgumentNullException.
        _ = HttpContext ?? throw new InvalidOperationException("HttpContent is null.");

        HttpResponseMessage res;
        try
        {
            // Create post body.
            PostAuthorizationContext context = new()
            {
                DisplayId = Model.DisplayId,
                PlanePassword = Model.PlainPassword
            };
            // TODO: Change login method to use ViewModel. 
            HttpClient httpClient = new();
            res = await httpClient.PostAsJsonAsync(new Uri("https://twihigh-dev-apim.azure-api.net/twihighusers/Login"), context, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch (Exception)
        {
            // TODO: タイムアウト時の処理を入れる
            throw;
        }

        res.EnsureSuccessStatusCode();
        ResponseJwtContext? jwt = await res.Content.ReadFromJsonAsync<ResponseJwtContext>();
        if (jwt == null || string.IsNullOrEmpty(jwt.Token))
        {
            throw new InvalidOperationException("Failed to login. Please check your username and password.");
        }

        Navigation.NavigateTo($"/cookiewriter/{jwt.Token}", replace: true);
    }
}
