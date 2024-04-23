using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Views.Pages;

public partial class Login : TwiHighPageBase
{
    private class LoginFormModel
    {
        [Required(ErrorMessage = "Display ID is required.")]
        public string DisplayId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string PlainPassword { get; set; } = string.Empty;
    }

    [SupplyParameterFromForm]
    private LoginFormModel Model { get; set; } = default!;

    private EditContext EditContext { get; set; } = default!;

    private ValidationMessageStore ValidationMessageStore { get; set; } = default!;

    protected override void OnInitialized()
    {
        Model ??= new LoginFormModel();
        EditContext = new EditContext(Model);
        ValidationMessageStore = new ValidationMessageStore(EditContext);
        if (HttpContext != null && HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
        {
            Navigation.NavigateTo("/home");
        }
    }

    private async Task HandleValidSubmitAsync(EditContext editContext)
    {
        _ = HttpContext ?? throw new InvalidOperationException("HttpContent is null.");
        HttpResponseMessage res;
        try
        {
            PostAuthorizationContext context = new()
            {
                DisplayId = Model.DisplayId,
                PlanePassword = Model.PlainPassword
            };
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
        //JwtSecurityToken jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwt.Token);
        Claim jwtClaim = new(nameof(JwtSecurityToken), jwt.Token);
        ClaimsIdentity identity = new([jwtClaim], CookieAuthenticationDefaults.AuthenticationScheme);
        // ログイン成功！
        await HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        Navigation.NavigateTo("/home");
    }
}
