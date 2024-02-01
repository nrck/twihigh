using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Net.Http.Json;
using static PheasantTails.TwiHigh.Data.Model.TwiHighUsers.PatchTwiHighUserContext;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class ProfileEditerViewModel : ViewModelBase, IProfileEditerViewModel
{
    private readonly TwiHighAuthenticationStateProvider _authenticationStateProvider;
    private PatchTwiHighUserContext _patchContext;
    private readonly string _apiUrlPatchTwihighUser;
    private readonly string _apiUrlGetTwihighUser;
    private readonly HttpClient _httpClient;

    public ReactivePropertySlim<ResponseTwiHighUserContext?> User { get; private set; } = default!;
    public ReactivePropertySlim<string> HeaderTitle { get; private set; } = default!;
    public ReactivePropertySlim<string> AvatarUrl { get; private set; } = default!;
    public ReactivePropertySlim<string> DisplayId { get; private set; } = default!;
    public ReactivePropertySlim<string> DisplayName { get; private set; } = default!;
    public ReactivePropertySlim<string> Biography { get; private set; } = default!;
    public ReactivePropertySlim<Base64EncodedFileContent?> LocalRawAvatarContent { get; private set; } = default!;
    public ReactivePropertySlim<bool> CanExecuteSaveCommand { get; private set; } = default!;

    public AsyncReactiveCommand<InputFileChangeEventArgs> LoadLocalFileCommand { get; private set; } = default!;
    public AsyncReactiveCommand SaveCommand { get; private set; } = default!;
    public ReactiveCommandSlim AvatarResetCommand { get; private set; } = default!;
    public AsyncReactiveCommand GetUserCommand { get; private set; } = default!;

    public ProfileEditerViewModel(HttpClient httpClient, IConfiguration configuration, AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager, IMessageService messageService)
        : base(navigationManager, messageService)
    {
        _patchContext = new();
        _authenticationStateProvider = (TwiHighAuthenticationStateProvider)authenticationStateProvider;
        _httpClient = httpClient;
        _apiUrlPatchTwihighUser = $"{configuration["AppUserApiUrl"]}/TwiHighUser/";
        _apiUrlGetTwihighUser = $"{configuration["AppUserApiUrl"]}/TwiHighUser/{{0}}";
    }

    protected override void Initialize()
    {
        User = new ReactivePropertySlim<ResponseTwiHighUserContext?>().AddTo(_disposable);
        HeaderTitle = new ReactivePropertySlim<string>().AddTo(_disposable);
        AvatarUrl = new ReactivePropertySlim<string>().AddTo(_disposable);
        DisplayId = new ReactivePropertySlim<string>().AddTo(_disposable);
        DisplayName = new ReactivePropertySlim<string>().AddTo(_disposable);
        Biography = new ReactivePropertySlim<string>().AddTo(_disposable);
        LocalRawAvatarContent = new ReactivePropertySlim<Base64EncodedFileContent?>().AddTo(_disposable);
        CanExecuteSaveCommand = new ReactivePropertySlim<bool>().AddTo(_disposable);

        LoadLocalFileCommand = new AsyncReactiveCommand<InputFileChangeEventArgs>().AddTo(_disposable);
        SaveCommand = new AsyncReactiveCommand(CanExecuteSaveCommand).AddTo(_disposable);
        AvatarResetCommand = new ReactiveCommandSlim().AddTo(_disposable);
        GetUserCommand = new AsyncReactiveCommand().AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        LoadLocalFileCommand.Subscribe(LoadFilesAsync);
        SaveCommand.Subscribe(SaveAsync);
        AvatarResetCommand.Subscribe(AvatarReset);
        GetUserCommand.Subscribe(GetTwiHighUserAsync);
    }

    private async Task LoadFilesAsync(InputFileChangeEventArgs e)
    {
        IBrowserFile file = e.File;
        if (file.IsSupportedImage() == false)
        {
            _messageService.SetWarnMessage("画像はPNGもしくはJPEGのみがサポートされています。他の画像を使用してください。");
            return;
        }
        if (file.IsEmpty())
        {
            _messageService.SetErrorMessage("画像データを読み込めませんでした。");
            return;
        }
        if (file.IsSupportedMaximumSize() == false)
        {
            _messageService.SetWarnMessage("画像の最大サイズは5MBです。リサイズするなど、ファイルサイズを小さくしてください。");
            return;
        }
        LocalRawAvatarContent.Value = await file.ToBase64EncodedFileContentAsync();
        AvatarUrl.Value = $"data:{LocalRawAvatarContent.Value.ContentType};base64,{LocalRawAvatarContent.Value.Data}";
    }

    private async Task SaveAsync()
    {
        if (!AdjustPatchContext())
        {
            _messageService.SetInfoMessage("プロフィールを変更してから保存するボタンを押してください。");
            return;
        }

        try
        {
            HttpResponseMessage res = await _httpClient.PatchAsJsonAsync(_apiUrlPatchTwihighUser, _patchContext);
            res.EnsureSuccessStatusCode();
            User.Value = await res.Content.ReadFromJsonAsync<ResponseTwiHighUserContext>();
            _messageService.SetSucessMessage("プロフィールを更新しました！");
            await _authenticationStateProvider.RefreshAuthenticationStateAsync();
            SetDisplayVariables();
        }
        catch (Exception)
        {
            _messageService.SetErrorMessage("プロフィールの更新に失敗しました。");
        }
    }

    private void AvatarReset()
    {
        LocalRawAvatarContent.Value = null;
        AvatarUrl.Value = User.Value?.AvatarUrl ?? string.Empty;
    }

    private async Task GetTwiHighUserAsync()
    {
        string id = await _authenticationStateProvider.GetLoggedInUserIdAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(id))
        {
            await _authenticationStateProvider.MarkUserAsLoggedOutAsync();
            _navigationManager.NavigateToLoginPage();
        }
        string url = string.Format(_apiUrlGetTwihighUser, id);
        try
        {
            User.Value = await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext>(url);
            SetDisplayVariables();
        }
        catch (Exception)
        {
            await _authenticationStateProvider.MarkUserAsLoggedOutAsync();
            _navigationManager.NavigateToLoginPage();
        }
    }

    private bool AdjustPatchContext()
    {
        bool isChanged = false;
        _patchContext = new PatchTwiHighUserContext();
        if (DisplayName.Value != User.Value?.DisplayName)
        {
            _patchContext.DisplayName = DisplayName.Value;
            isChanged = true;
        }
        if (DisplayId.Value != User.Value?.DisplayId)
        {
            _patchContext.DisplayId = DisplayId.Value;
            isChanged = true;
        }
        if (Biography.Value != User.Value?.Biography)
        {
            _patchContext.Biography = Biography.Value;
            isChanged = true;
        }
        if (LocalRawAvatarContent.Value != null && !string.IsNullOrEmpty(LocalRawAvatarContent.Value.Data) && !string.IsNullOrEmpty(LocalRawAvatarContent.Value.ContentType))
        {
            _patchContext.Base64EncodedAvatarImage = LocalRawAvatarContent.Value;
            isChanged = true;
        }

        return isChanged;
    }

    private void SetDisplayVariables()
    {
        HeaderTitle.Value = $"{User.Value?.DisplayName}（@{User.Value?.DisplayId}）";
        AvatarUrl.Value = User.Value?.AvatarUrl ?? string.Empty;
        DisplayId.Value = User.Value?.DisplayId ?? string.Empty;
        DisplayName.Value = User.Value?.DisplayName ?? string.Empty;
        Biography.Value = User.Value?.Biography ?? string.Empty;
    }
}
