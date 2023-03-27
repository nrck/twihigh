using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Data.Store.Entity;
using System.Net;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Home : PageBase, IDisposable, IAsyncDisposable
    {
        private Tweet[] Tweets { get; set; } = Array.Empty<Tweet>();

        private CancellationTokenSource? WorkerCancellationTokenSource { get; set; } = null;

        private string AvatarUrl { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            WorkerCancellationTokenSource ??= new CancellationTokenSource();
            AvatarUrl = await GetMyAvatarUrlAsync();
            StateHasChanged();

            await GetMyTimerlineEvery5secAsync(WorkerCancellationTokenSource.Token);
        }

        private async Task GetMyTimerlineEvery5secAsync(CancellationToken cancellationToken = default)
        {
            ResponseTimelineContext? response = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    response = await TimelineHttpClient.GetMyTimelineAsync();
                }
                catch (HttpRequestException ex)
                {
                    switch (ex.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOutAsync();
                            Navigation.NavigateTo(DefinePaths.PAGE_PATH_LOGIN);
                            break;
                        default:
                            break;
                    }
                }

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorkerCancellationTokenSource?.Cancel();
                base.Dispose(disposing);
            }
        }
    }
}
