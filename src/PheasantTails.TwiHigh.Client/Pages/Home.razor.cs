using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.Timelines;
using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Home : IDisposable
    {
        private readonly ResponseTimelineContext TEST = new ResponseTimelineContext
        {
            Latest = DateTimeOffset.UtcNow,
            Oldest = DateTimeOffset.UtcNow,
            Tweets = new Tweet[]
            {
                new Tweet
                {
                    Id = Guid.NewGuid(),
                    CreateAt = DateTimeOffset.UtcNow,
                    ReplyFrom = Array.Empty<Guid>(),
                    ReplyTo = null,
                    Text = "テストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイートテストツイート",
                    UserAvatarUrl = "https://pbs.twimg.com/profile_images/1547938967257251840/Y0IVpcxC_400x400.jpg",
                    UserDisplayId = "nr_ck",
                    UserDisplayName = "けー【新刊はBOOTH】",
                    UserId = Guid.NewGuid(),
                }
            }
        };

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private TimelineHttpClient TimelineHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private ILocalStorageService LocalStorage { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        private Tweet[] Tweets { get; set; } = Array.Empty<Tweet>();

        private CancellationTokenSource? WorkerCancellationTokenSource { get; set; } = null;

        protected override async Task OnInitializedAsync()
        {
            WorkerCancellationTokenSource ??= new CancellationTokenSource();
            var token = await LocalStorage.GetItemAsStringAsync("TwiHighJwt");
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

        public void Dispose()
        {
            WorkerCancellationTokenSource?.Cancel();
            GC.SuppressFinalize(this);
        }
    }
}
