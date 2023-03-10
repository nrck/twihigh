using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class PostFormComponent
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private TweetHttpClient TweetHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private ILocalStorageService LocalStorage { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;

        public PostTweetContext PostTweetContext { get; set; } = new PostTweetContext();

        protected override async Task OnInitializedAsync()
        {
            var token = await LocalStorage.GetItemAsStringAsync("TwiHighJwt");
            TweetHttpClient.SetToken(token);
            await base.OnInitializedAsync();
        }

        private async Task OnSubmitAsync()
        {
            await TweetHttpClient.PostTweetAsync(PostTweetContext);
            Console.WriteLine("OnSubmit:" + PostTweetContext.Text);
            PostTweetContext.Text = string.Empty;
            PostTweetContext.ReplyTo = null;
        }
    }
}
