using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using System.Collections.ObjectModel;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components
{
    public partial class THTimeline : TwiHighComponentBase
    {
        /// <summary>
        /// タイムラインに表示するツイートList
        /// </summary>
        [Parameter]
        public ReadOnlyCollection<DisplayTweet>? Tweets { get; set; }

        [Parameter]
        public Guid MyTwiHithUserId { get; set; }

        /// <summary>
        /// リプライが投稿されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<PostTweetContext> OnPostReply { get; set; }

        /// <summary>
        /// ツイート削除ボタンが押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<DisplayTweet> OnClickDelete { get; set; }

        /// <summary>
        /// ツイート詳細が押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<DisplayTweet> OnClickDetail { get; set; }

        /// <summary>
        /// リツイートが押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<DisplayTweet> OnClickRetweet { get; set; }

        /// <summary>
        /// お気に入りが押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<DisplayTweet> OnClickFavorite { get; set; }

        /// <summary>
        /// プロフィール欄への遷移が発生したときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<DisplayTweet> OnClickProfile { get; set; }

        /// <summary>
        /// プロフィール編集欄への遷移が発生したときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback OnClickProfileEditor { get; set; }

        /// <summary>
        /// 更にツイートを取得ボタンが押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<DisplayTweet> OnClickGapTweetsLoad { get; set; }

        private Task OnClickReplyAsync(PostTweetContext tweet) => OnPostReply.InvokeAsync(tweet);

        private Task OnClickDeleteAsync(DisplayTweet tweet) => OnClickDelete.InvokeAsync(tweet);

        private Task OnClickDetailAsync(DisplayTweet tweet) => OnClickDetail.InvokeAsync(tweet);

        private Task OnClickRetweetAsync(DisplayTweet tweet) => OnClickRetweet.InvokeAsync(tweet);

        private Task OnClickFavoriteAsync(DisplayTweet tweet) => OnClickFavorite.InvokeAsync(tweet);

        private Task OnClickProfileAsync(DisplayTweet tweet) => OnClickProfile.InvokeAsync(tweet);

        private Task OnClickProfileEditorAsync() => OnClickProfileEditor.InvokeAsync();

        private Task OnClickGapTweetsLoadAsync(DisplayTweet tweet) => OnClickGapTweetsLoad.InvokeAsync(tweet);
    }
}
