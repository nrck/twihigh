using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.ViewModels;
using PheasantTails.TwiHigh.Data.Model.Tweets;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class TimelineComponent : UIComponentBase
    {
        /// <summary>
        /// タイムラインに表示するツイートList
        /// </summary>
        [Parameter]
        public List<TweetViewModel> Tweets { get; set; }

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
        public EventCallback<TweetViewModel> OnClickDelete { get; set; }

        /// <summary>
        /// ツイート詳細が押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<TweetViewModel> OnClickDetail { get; set; }

        /// <summary>
        /// リツイートが押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<TweetViewModel> OnClickRetweet { get; set; }

        /// <summary>
        /// お気に入りが押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<TweetViewModel> OnClickFavorite { get; set; }

        /// <summary>
        /// プロフィール欄への遷移が発生したときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<TweetViewModel> OnClickProfile { get; set; }

        /// <summary>
        /// プロフィール編集欄への遷移が発生したときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback OnClickProfileEditor { get; set; }

        /// <summary>
        /// 更にツイートを取得ボタンが押下されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public EventCallback<TweetViewModel> OnClickGapTweetsLoad { get; set; }

        private Task OnClickReplyAsync(PostTweetContext tweet) => OnPostReply.InvokeAsync(tweet);

        private Task OnClickDeleteAsync(TweetViewModel tweet) => OnClickDelete.InvokeAsync(tweet);

        private Task OnClickDetailAsync(TweetViewModel tweet) => OnClickDetail.InvokeAsync(tweet);

        private Task OnClickRetweetAsync(TweetViewModel tweet) => OnClickRetweet.InvokeAsync(tweet);

        private Task OnClickFavoriteAsync(TweetViewModel tweet) => OnClickFavorite.InvokeAsync(tweet);

        private Task OnClickProfileAsync(TweetViewModel tweet) => OnClickProfile.InvokeAsync(tweet);

        private Task OnClickProfileEditorAsync() => OnClickProfileEditor.InvokeAsync();

        private Task OnClickGapTweetsLoadAsync(TweetViewModel tweet) => OnClickGapTweetsLoad.InvokeAsync(tweet);
    }
}
