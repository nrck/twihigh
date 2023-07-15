using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Client.Pages;
using PheasantTails.TwiHigh.Client.ViewModels;
using PheasantTails.TwiHigh.Data.Model;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class TweetComponent : UIComponentBase
    {
        /// <summary>
        /// 表示するツイート
        /// </summary>
        [Parameter]
        public TweetViewModel? Tweet { get; set; }

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
        /// 表示しているツイートが自身のものなら<c>true</c>
        /// </summary>
        [Parameter]
        public bool IsMyTweet { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private bool IsOpendReplyPostForm { get; set; }

        private ReplyToContext? _replyToContext;
        private ReplyToContext? ReplyToContext
        {
            get
            {
                if (_replyToContext == null && Tweet != null)
                {
                    _replyToContext = new ReplyToContext
                    {
                        TweetId = Tweet.Id,
                        UserId = Tweet.UserId
                    };
                }

                return _replyToContext;
            }
        }

        private string ArticleId => $"tweet-{Tweet?.Id}";

        private PostFormComponent ReplySection { get; set; }

        private bool IsAuthenticated { get; set; }

        protected override Task OnInitializedAsync()
        {
            return Task.Run(async () =>
            {
                IsAuthenticated = await GetIsAuthenticatedAsync();
                await base.OnInitializedAsync();
            });
        }

        protected override void OnParametersSet()
        {
            IsOpendReplyPostForm = IsAuthenticated && (Tweet?.IsOpendReplyPostForm ?? false);
            base.OnParametersSet();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            var focusTask = Task.Run(async () =>
            {
                await Task.Delay(500);
                if (IsOpendReplyPostForm && ReplySection != null)
                {
                    await ReplySection.TextAreaFocusAsync();
                }
            });
            var baseTask = base.OnAfterRenderAsync(firstRender);

            return Task.WhenAll(focusTask, baseTask);
        }

        private Task OnClickAvatar(MouseEventArgs _) => OnClickProfile.InvokeAsync(Tweet);

        private Task OnClickUserDisplayName(MouseEventArgs _) => OnClickProfile.InvokeAsync(Tweet);

        private Task OnClickUserDisplayId(MouseEventArgs _) => OnClickProfile.InvokeAsync(Tweet);

        private Task OnClickDeleteButton(MouseEventArgs _) => OnClickDelete.InvokeAsync(Tweet);

        private void OnClickReplyButton(MouseEventArgs _)
        {
            if (Tweet == null)
            {
                return;
            }
            if (Tweet.IsEmphasized)
            {
                IsOpendReplyPostForm = IsAuthenticated && !IsOpendReplyPostForm;
                return;
            }
            var url = string.Format(DefinePaths.PAGE_PATH_STATUS, Tweet.UserDisplayId, Tweet.Id);
            NavigationManager.NavigateTo($"{url}/reply");
        }

        private async Task OnClickPostTweet(PostTweetContext postTweet)
        {
            await OnPostReply.InvokeAsync(postTweet);
            IsOpendReplyPostForm = false;
        }

        private void OnClickTweetArea(MouseEventArgs _) => OnClickDetail.InvokeAsync(Tweet);
    }
}
