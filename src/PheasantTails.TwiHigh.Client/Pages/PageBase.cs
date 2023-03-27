using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        protected override void OnInitialized()
        {
            LocalStorageService.Changed += OnChangeTwiHighJwt;
            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            await SetTokenToHtmlClient();
            await base.OnInitializedAsync();
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return base.OnAfterRenderAsync(firstRender);
        }

        protected async Task SetTokenToHtmlClient()
        {
            var token = await LocalStorageService.GetItemAsStringAsync(LOCAL_STORAGE_KEY_JWT);
            SetTokenToHtmlClient(token);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                LocalStorageService.Changed -= OnChangeTwiHighJwt;
            }
        }

        private void OnChangeTwiHighJwt(object? _, ChangedEventArgs e)
        {
            if(e.Key != LOCAL_STORAGE_KEY_JWT)
            {
                return;
            }
            var token = e.NewValue.ToString() ?? string.Empty;
            SetTokenToHtmlClient(token);
        }

        private void SetTokenToHtmlClient(string token)
        {
            AppUserHttpClient.SetToken(token);
            FollowHttpClient.SetToken(token);
            TimelineHttpClient.SetToken(token);
            TweetHttpClient.SetToken(token);
        }
    }
}
