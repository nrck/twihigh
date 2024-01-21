using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Net.Http.Json;
using System.Text.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class LoginViewModel : ViewModelBase, ILoginViewModel
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly HttpClient _httpClient;
    private readonly string _apiUrlLogin;

    public ReactivePropertySlim<string> DisplayId { get; } = new ReactivePropertySlim<string>();
    public ReactivePropertySlim<string> PlainPassword { get; } = new ReactivePropertySlim<string>();
    public ReactivePropertySlim<bool> CanExecute { get; } = new ReactivePropertySlim<bool>();
    public AsyncReactiveCommand<TwiHighUIBase> LoginCommand { get; } = new AsyncReactiveCommand<TwiHighUIBase>();
    public AsyncReactiveCommand CheckAuthenticationStateCommand { get; } = new AsyncReactiveCommand();

    public LoginViewModel(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, IConfiguration configuration, NavigationManager navigationManager, IMessageService messageService) : base(navigationManager, messageService)
    {
        _httpClient = httpClient;
        _apiUrlLogin = $"{configuration["AppUserApiUrl"]}/Login";
        _authenticationStateProvider = authenticationStateProvider;

        DisplayId.AddTo(_disposable);
        PlainPassword.AddTo(_disposable);
        CanExecute.AddTo(_disposable);
        LoginCommand.AddTo(_disposable);
        CheckAuthenticationStateCommand.AddTo(_disposable);

        CanExecute.Value = true;
    }

    protected override void Subscribe()
    {
        LoginCommand.WithSubscribe(async (component) =>
        {
            CanExecute.Value = false;
            await component.InvokeRenderAsync();
            await LoginAsync();
            CanExecute.Value = true;
            await component.InvokeRenderAsync();
        });

        CheckAuthenticationStateCommand.WithSubscribe(async () =>
        {
            await CheckAuthenticationStateAsync();
        });
    }

    private async ValueTask LoginAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(DisplayId.Value))
            {
                _messageService.SetErrorMessage("ユーザ名を入力してください。");
                return;
            }
            if (string.IsNullOrEmpty(PlainPassword.Value))
            {
                _messageService.SetErrorMessage("パスワードを入力してください。");
                return;
            }
            HttpResponseMessage res;
            try
            {
                var context = new PostAuthorizationContext
                {
                    DisplayId = DisplayId.Value,
                    PlanePassword = PlainPassword.Value
                };
                res = await _httpClient.PostAsJsonAsync(new Uri(_apiUrlLogin), context, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                // TODO: タイムアウト時の処理を入れる
                throw;
            }

            if (!res.IsSuccessStatusCode)
            {
                _messageService.SetErrorMessage($"ログインできませんでした。HTTPステータス：{(int)res.StatusCode}");
                return;
            }
            var jwt = await res.Content.ReadFromJsonAsync<ResponseJwtContext>(cancellationToken: cancellationToken);
            if (jwt == null || string.IsNullOrEmpty(jwt.Token))
            {
                _messageService.SetErrorMessage("ログインできませんでした。ユーザ名とパスワードを確認してください。");
                return;
            }

            await ((TwiHighAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticatedAsync(jwt.Token, cancellationToken: cancellationToken);
            _messageService.SetInfoMessage("ログインしました。");
            _navigationManager.NavigateToHomePage(false, true);
            return;
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private async ValueTask CheckAuthenticationStateAsync()
    {
        try
        {
            AuthenticationState state = await ((TwiHighAuthenticationStateProvider)_authenticationStateProvider).GetAuthenticationStateAsync().ConfigureAwait(false);
            var isAuthenticated = state.User.Identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
            {
                _navigationManager.NavigateToHomePage(false, true);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }
}
