using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components
{
    public partial class THTweet : TwiHighComponentBase
    {
        /// <summary>
        /// 表示するツイート
        /// </summary>
        [Parameter]
        public DisplayTweet? Tweet { get; set; }

        /// <summary>
        /// リプライが投稿されたときに発火するイベントコールバック
        /// </summary>
        [Parameter]
        public AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; set; } = default!;

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
        /// 表示しているツイートが自身のものなら<c>true</c>
        /// </summary>
        [Parameter]
        public bool IsMyTweet { get; set; }

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

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        private THPostForm ReplySection { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        private bool IsAuthenticated { get; set; }

        protected override Task OnInitializedAsync()
        {
            return Task.Run(async () =>
            {
                AuthenticationState state = await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).GetAuthenticationStateAsync();
                IsAuthenticated = state.User.Identity?.IsAuthenticated ?? false;
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
            Task focusTask = Task.Run(async () =>
            {
                await Task.Delay(500);
                if (IsOpendReplyPostForm && ReplySection != null)
                {
                    await ReplySection.TextAreaFocusAsync();
                }
            });
            Task baseTask = base.OnAfterRenderAsync(firstRender);

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
            Navigation.NavigateToStatePage(Tweet, true);
        }

        private void OnClickTweetArea(MouseEventArgs _) => OnClickDetail.InvokeAsync(Tweet);
    }
}
