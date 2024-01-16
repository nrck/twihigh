using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Specialized;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class LoginViewModel : ViewModelBase, IDisposable, INotifyCollectionChanged
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly HttpClient _httpClient;
    private readonly string _apiUrlLogin;

    public ReactivePropertySlim<PostAuthorizationContext> AuthorizationContext { get; }
    public AsyncReactiveCommand LoginCommand { get; }
    public AsyncReactiveCommand CheckAuthenticationStateCommand { get; }

    public LoginViewModel(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, IConfiguration configuration, NavigationManager navigationManager, IMessageService messageService) : base(navigationManager, messageService)
    {
        _httpClient = httpClient;
        _apiUrlLogin = $"{configuration["AppUserApiUrl"]}/Login";
        _authenticationStateProvider = authenticationStateProvider;
        AuthorizationContext = new ReactivePropertySlim<PostAuthorizationContext>();
        LoginCommand = new AsyncReactiveCommand()
            .WithSubscribe(async () =>
            {
                await LoginAsync();
            })
            .AddTo(_disposable);
        CheckAuthenticationStateCommand = new AsyncReactiveCommand()
            .WithSubscribe(async () =>
            {
                await CheckAuthenticationStateAsync();
            })
            .AddTo(_disposable);
    }

    private async ValueTask LoginAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(AuthorizationContext.Value.DisplayId))
            {
                _messageService.SetErrorMessage("ユーザ名を入力してください。");
                return;
            }
            if (string.IsNullOrEmpty(AuthorizationContext.Value.PlanePassword))
            {
                _messageService.SetErrorMessage("パスワードを入力してください。");
                return;
            }
            HttpResponseMessage res;
            try
            {
                res = await _httpClient.PostAsJsonAsync(_apiUrlLogin, AuthorizationContext.Value, cancellationToken: cancellationToken);
            }
            catch (Exception)
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
