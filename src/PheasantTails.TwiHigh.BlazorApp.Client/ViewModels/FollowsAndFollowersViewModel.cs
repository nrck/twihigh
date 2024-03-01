using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Net.Http.Json;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class FollowsAndFollowersViewModel : ViewModelBase, IFollowersViewModel, IFollowsViewModel
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrlBase;
    private readonly string _apiUrlGetTwihighUser;
    private readonly string _apiUrlGetTwihighUserFollowers;
    private readonly string _apiUrlGetTwihighUserFollows;

    public ReactivePropertySlim<string> PageTitle { get; private set; } = default!;
    public ReactivePropertySlim<ResponseTwiHighUserContext?> UserDisplayedOnScreen { get; private set; } = default!;
    public ReactiveCollection<ResponseTwiHighUserContext> UserFollowers { get; private set; } = default!;
    public ReactiveCollection<ResponseTwiHighUserContext> UserFollows { get; private set; } = default!;
    public ReactivePropertySlim<bool> CanExequteGetTwiHighUserFollowersCommand { get; private set; } = default!;
    public ReactivePropertySlim<bool> CanExequteGetTwiHighUserFollowsCommand { get; private set; } = default!;
    public AsyncReactiveCommand<string> GetTwiHighUserFollowersCommand { get; private set; } = default!;
    public AsyncReactiveCommand<string> GetTwiHighUserFollowsCommand { get; private set; } = default!;


    public FollowsAndFollowersViewModel(HttpClient httpClient, IConfiguration configuration, NavigationManager navigationManager, IMessageService messageService)
        : base(navigationManager, messageService)
    {
        _httpClient = httpClient;
        _apiUrlBase = $"{configuration["AppUserApiUrl"]}";
        _apiUrlGetTwihighUser = $"{_apiUrlBase}/TwiHighUser/{{0}}";
        _apiUrlGetTwihighUserFollows = $"{_apiUrlBase}/TwiHighUser/{{0}}/Follows";
        _apiUrlGetTwihighUserFollowers = $"{_apiUrlBase}/TwiHighUser/{{0}}/Followers";
    }

    protected override void Initialize()
    {
        PageTitle = new ReactivePropertySlim<string>("プロフィール読み込み中").AddTo(_disposable);
        UserDisplayedOnScreen = new ReactivePropertySlim<ResponseTwiHighUserContext?>().AddTo(_disposable);
        UserFollowers = new ReactiveCollection<ResponseTwiHighUserContext>().AddTo(_disposable);
        UserFollows = new ReactiveCollection<ResponseTwiHighUserContext>().AddTo(_disposable);
        CanExequteGetTwiHighUserFollowersCommand = new ReactivePropertySlim<bool>(true).AddTo(_disposable);
        CanExequteGetTwiHighUserFollowsCommand = new ReactivePropertySlim<bool>(true).AddTo(_disposable);
        GetTwiHighUserFollowersCommand = new AsyncReactiveCommand<string>(CanExequteGetTwiHighUserFollowersCommand).AddTo(_disposable);
        GetTwiHighUserFollowsCommand = new AsyncReactiveCommand<string>(CanExequteGetTwiHighUserFollowsCommand).AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        GetTwiHighUserFollowersCommand.Subscribe(GetTwiHighUserFollowersAsync);
        GetTwiHighUserFollowsCommand.Subscribe(GetTwiHighUserFollowsAsync);
    }

    private async Task GetTwiHighUserFollowersAsync(string id)
    {
        ValiableInitialize();
        UserDisplayedOnScreen.Value = await GetTwiHighUserAsync(id).ConfigureAwait(false);
        SetPageTitle(false);
        string url = string.Format(_apiUrlGetTwihighUserFollowers, id);
        ResponseTwiHighUserContext[] users = await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext[]>(url) ?? [];
        UserFollowers.AddRangeOnScheduler(users);
    }

    private async Task GetTwiHighUserFollowsAsync(string id)
    {
        ValiableInitialize();
        UserDisplayedOnScreen.Value = await GetTwiHighUserAsync(id).ConfigureAwait(false);
        SetPageTitle(true);
        string url = string.Format(_apiUrlGetTwihighUserFollows, id);
        ResponseTwiHighUserContext[] users = await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext[]>(url) ?? [];
        UserFollows.AddRangeOnScheduler(users);
    }

    private async Task<ResponseTwiHighUserContext?> GetTwiHighUserAsync(string id)
    {
        string urlGetTwihighUser = string.Format(_apiUrlGetTwihighUser, id);
        return await _httpClient.GetFromJsonAsync<ResponseTwiHighUserContext>(urlGetTwihighUser).ConfigureAwait(false);
    }

    private void SetPageTitle(bool isFollow)
    {
        if (UserDisplayedOnScreen.Value == null)
        {
            PageTitle.Value = "プロフィールを読み込めませんでした。";
        }
        else
        {
            PageTitle.Value = $"{UserDisplayedOnScreen.Value.DisplayName}（@{UserDisplayedOnScreen.Value.DisplayId}）の{(isFollow ? "フォロー" : "フォロワー")}";
        }
    }

    private void ValiableInitialize()
    {
        PageTitle.Value = "プロフィール読み込み中";
        UserDisplayedOnScreen.Value = null;
        UserFollowers.ClearOnScheduler();
    }
}
