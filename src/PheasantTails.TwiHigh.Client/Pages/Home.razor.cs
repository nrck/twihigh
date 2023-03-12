using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Home : IDisposable
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private TimelineHttpClient TimelineHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private AppUserHttpClient AppUserHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private ILocalStorageService LocalStorage { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationState { get; set; }

        private Tweet[] Tweets { get; set; } = Array.Empty<Tweet>();

        private CancellationTokenSource? WorkerCancellationTokenSource { get; set; } = null;

        private string AvatarUrl { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            WorkerCancellationTokenSource ??= new CancellationTokenSource();
            var token = await LocalStorage.GetItemAsStringAsync("TwiHighJwt");
            AvatarUrl = await GetMyAvatarUrlAsync();
            TimelineHttpClient.SetToken(token);

            await Task.WhenAll(
                base.OnInitializedAsync(),
                GetMyTimerlineEvery5secAsync(WorkerCancellationTokenSource.Token)
            );
        }

        private async Task GetMyTimerlineEvery5secAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await TimelineHttpClient.GetMyTimelineAsync();
                if (response != null)
                {
                    Tweets = response.Tweets;
                    StateHasChanged();
                }

                await Task.Delay(5000, cancellationToken);
            }
        }

        private async Task<string> GetMyAvatarUrlAsync()
        {
            if (AuthenticationState is null)
            {
                await Task.Delay(5000);
                await GetMyAvatarUrlAsync();
                return string.Empty;
            }
            return (await AuthenticationState).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.AvatarUrl))?.Value ?? string.Empty;
        }

        public void Dispose()
        {
            WorkerCancellationTokenSource?.Cancel();
            GC.SuppressFinalize(this);
        }
    }
}
