using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.Client.Extensions;
using PheasantTails.TwiHigh.Client.TypedHttpClients;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public class PageBase : ComponentBase, IDisposable, IAsyncDisposable
    {
        private const string LOCAL_STORAGE_KEY_JWT = "TwiHighJwt";

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        protected NavigationManager Navigation { get; set; }

        [Inject]
        protected ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        protected ILogger<PageBase> Logger { get; set; }

        [Inject]
        protected AppUserHttpClient AppUserHttpClient { get; set; }

        [Inject]
        protected FollowHttpClient FollowHttpClient { get; set; }

        [Inject]
        protected TimelineHttpClient TimelineHttpClient { get; set; }

        [Inject]
        protected TweetHttpClient TweetHttpClient { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        [CascadingParameter]
        protected Task<AuthenticationState>? AuthenticationState { get; set; }

        public void Dispose()
        {
            Logger.LogStart();
            Dispose(true);
            GC.SuppressFinalize(this);
            Logger.LogFinish();
        }

        public ValueTask DisposeAsync()
        {
            Logger.LogStart();
            Dispose(true);
            GC.SuppressFinalize(this);
            Logger.LogFinish();

            return ValueTask.CompletedTask;
        }

        protected override void OnInitialized()
        {
            Logger.LogStart();
            LocalStorageService.Changed += OnChangeTwiHighJwt;
            base.OnInitialized();
            Logger.LogFinish();
        }

        protected override async Task OnInitializedAsync()
        {
            Logger.LogStart();
            await SetTokenToHtmlClient();
            await base.OnInitializedAsync();
            Logger.LogFinish();
        }

        protected override void OnParametersSet()
        {
            Logger.LogStart();
            base.OnParametersSet();
            Logger.LogFinish();
        }

        protected override Task OnParametersSetAsync()
        {
            Logger.LogStart();
            var task = base.OnParametersSetAsync();
            Logger.LogFinish();

            return task;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            Logger.LogStart();
            base.OnAfterRender(firstRender);
            Logger.LogFinish();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            Logger.LogStart();
            var task = base.OnAfterRenderAsync(firstRender);
            Logger.LogFinish();

            return task;
        }

        protected async Task SetTokenToHtmlClient()
        {
            Logger.LogStart();
            var token = await LocalStorageService.GetItemAsStringAsync(LOCAL_STORAGE_KEY_JWT);
            SetTokenToHtmlClient(token);
            Logger.LogFinish();
        }

        protected virtual void Dispose(bool disposing)
        {
            Logger.LogStart();
            if (disposing)
            {
                LocalStorageService.Changed -= OnChangeTwiHighJwt;
            }
            Logger.LogFinish();
        }

        private void OnChangeTwiHighJwt(object? _, ChangedEventArgs e)
        {
            Logger.LogStart();
            if (e.Key != LOCAL_STORAGE_KEY_JWT)
            {
                Logger.LogFinish();
                return;
            }
            var token = e.NewValue.ToString() ?? string.Empty;
            SetTokenToHtmlClient(token);
            Logger.LogFinish();
        }

        private void SetTokenToHtmlClient(string token)
        {
            Logger.LogStart();
            if (string.IsNullOrEmpty(token))
            {
                Navigation.NavigateTo(DefinePaths.PAGE_PATH_LOGIN, replace: true);
                Logger.LogFinish();
                return;
            }

            AppUserHttpClient.SetToken(token);
            FollowHttpClient.SetToken(token);
            TimelineHttpClient.SetToken(token);
            TweetHttpClient.SetToken(token);
            Logger.LogFinish();
        }
    }
}
