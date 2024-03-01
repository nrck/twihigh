using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class SignupViewModel : ViewModelBase, ISignupViewModel
{
    /// <summary>
    /// Validator for the AddTwiHighUserContext.
    /// </summary>
    private readonly AddTwiHighUserContextValidator _validator = new();

    /// <summary>
    /// The authentication state provider for TwiHigh.
    /// </summary>
    private readonly TwiHighAuthenticationStateProvider _authenticationStateProvider;

    /// <summary>
    /// The HTTP client for making API requests.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// The base URL for the API.
    /// </summary>
    private readonly string _apiUrlBase;

    /// <summary>
    /// The URL for signing up a user.
    /// </summary>
    private readonly string _apiUrlSignin;

    /// <summary>
    /// The URL for logging in a user.
    /// </summary>
    private readonly string _apiUrlLogin;

    /// <summary>
    /// Gets or sets the reactive property for the signup command availability.
    /// </summary>
    public ReactivePropertySlim<bool> CanSignupCommand { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the reactive property for the display ID.
    /// </summary>
    public ReactivePropertySlim<string> DisplayId { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the reactive property for the password.
    /// </summary>
    public ReactivePropertySlim<string> Password { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the reactive property for the email.
    /// </summary>
    public ReactivePropertySlim<string> Email { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the async reactive command for the signup action.
    /// </summary>
    public AsyncReactiveCommand SignupCommand { get; private set; } = default!;

    public SignupViewModel(IConfiguration configuration, HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager, IMessageService messageService) : base(navigationManager, messageService)
    {
        _authenticationStateProvider = authenticationStateProvider as TwiHighAuthenticationStateProvider
            ?? throw new ArgumentException($"{nameof(authenticationStateProvider)} is not {nameof(TwiHighAuthenticationStateProvider)}", nameof(authenticationStateProvider));
        _httpClient = httpClient;
        _apiUrlBase = $"{configuration["AppUserApiUrl"]}";
        _apiUrlSignin = $"{_apiUrlBase}/SignUp";
        _apiUrlLogin = $"{_apiUrlBase}/Login";
    }

    protected override void Initialize()
    {
        // Property
        CanSignupCommand = new ReactivePropertySlim<bool>(true).AddTo(_disposable);
        DisplayId = new ReactivePropertySlim<string>().AddTo(_disposable);
        Password = new ReactivePropertySlim<string>().AddTo(_disposable);
        Email = new ReactivePropertySlim<string>().AddTo(_disposable);

        // Command
        SignupCommand = new AsyncReactiveCommand(CanSignupCommand).AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        SignupCommand.Subscribe(SignupAsync).AddTo(_disposable);
    }

    /// <summary>
    /// Signup a user.
    /// </summary>
    private async Task SignupAsync()
    {
        AddTwiHighUserContext context = new()
        {
            DisplayId = DisplayId.Value,
            DisplayName = DisplayId.Value,
            Password = Password.Value,
            Email = Email.Value
        };
        ValidationResult result = _validator.Validate(context);
        if (!result.IsValid)
        {
            _messageService.SetErrorMessage(result.Errors.FirstOrDefault()?.ErrorMessage ?? "入力項目を見直してください。");
            return;
        }

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiUrlSignin, context).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _messageService.SetErrorMessage($"すでに @{context.DisplayId} は使用されているようです。他のアカウントIDを使用してください。");
            return;
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _messageService.SetErrorMessage($"アカウントIDは必ず入力する必要があります。");
            return;
        }
        else if (!response.IsSuccessStatusCode)
        {
            _messageService.SetErrorMessage($"申し訳ありません。サーバーでエラーが発生しました。({response.StatusCode})");
            return;
        }

        PostAuthorizationContext loginContext = new()
        {
            DisplayId = context.DisplayId,
            PlanePassword = context.Password
        };

        response = await _httpClient.PostAsJsonAsync(_apiUrlLogin, loginContext).ConfigureAwait(false);
        ResponseJwtContext? jwt = await response.Content.TwiHighReadFromJsonAsync<ResponseJwtContext>().ConfigureAwait(false);
        if (jwt == null || string.IsNullOrEmpty(jwt.Token))
        {
            _messageService.SetErrorMessage($"ログインに失敗しました。");
            return;
        }

        await _authenticationStateProvider.MarkUserAsAuthenticatedAsync(jwt.Token);
        _navigationManager.NavigateToHomePage(replace: true);
    }
}
