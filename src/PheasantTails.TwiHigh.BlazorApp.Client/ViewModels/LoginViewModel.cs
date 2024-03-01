using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
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

    public ReactivePropertySlim<string> DisplayId { get; private set; } = default!;
    public ReactivePropertySlim<string> PlainPassword { get; private set; } = default!;
    public ReactivePropertySlim<bool> CanExecute { get; private set; } = default!;
    public AsyncReactiveCommand LoginCommand { get; private set; } = default!;
    public AsyncReactiveCommand CheckAuthenticationStateCommand { get; private set; } = default!;

    public LoginViewModel(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, IConfiguration configuration, NavigationManager navigationManager, IMessageService messageService)
        : base(navigationManager, messageService)
    {
        _httpClient = httpClient;
        _apiUrlLogin = $"{configuration["AppUserApiUrl"]}/Login";
        _authenticationStateProvider = authenticationStateProvider;

    }

    protected override void Initialize()
    {
        DisplayId = new ReactivePropertySlim<string>().AddTo(_disposable);
        PlainPassword = new ReactivePropertySlim<string>().AddTo(_disposable);
        CanExecute = new ReactivePropertySlim<bool>(true).AddTo(_disposable);
        LoginCommand = new AsyncReactiveCommand(CanExecute).AddTo(_disposable);
        CheckAuthenticationStateCommand = new AsyncReactiveCommand().AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        LoginCommand.Subscribe(async () => await LoginAsync());
        CheckAuthenticationStateCommand.Subscribe(async () => await CheckAuthenticationStateAsync());
    }

    private async Task LoginAsync(CancellationToken cancellationToken = default)
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
                PostAuthorizationContext context = new()
                {
                    DisplayId = DisplayId.Value,
                    PlanePassword = PlainPassword.Value
                };
                res = await _httpClient.PostAsJsonAsync(new Uri(_apiUrlLogin), context, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }, cancellationToken: cancellationToken);
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
            ResponseJwtContext? jwt = await res.Content.ReadFromJsonAsync<ResponseJwtContext>(cancellationToken: cancellationToken);
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
            bool isAuthenticated = state.User.Identity?.IsAuthenticated ?? false;
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
