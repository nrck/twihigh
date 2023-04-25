using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Client.Pages;
using PheasantTails.TwiHigh.Data.Model;
using PheasantTails.TwiHigh.Data.Store.Entity;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class TweetComponent
    {
        [Parameter]
        public Tweet? Tweet { get; set; }

        [Parameter]
        public bool IsMyTweet { get; set; }

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Parameter]
        public Action<Guid> OnDeleteAction { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private NavigationManager Navigation { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        private string CreateAt
        {
            get
            {
                if (Tweet == null)
                {
                    return string.Empty;
                }

                // 1年前ならyyyy/mm/dd
                if (Tweet.CreateAt <= DateTimeOffset.UtcNow.AddYears(-1))
                {
                    return Tweet.CreateAt.ToLocalTime().ToString("yyyy/MM/dd");
                }

                // 24時間より前ならm/d
                if (Tweet.CreateAt <= DateTimeOffset.UtcNow.AddDays(-1))
                {
                    return Tweet.CreateAt.ToLocalTime().ToString("M/d");
                }

                // 1時間より前なら H:mm
                if (Tweet.CreateAt <= DateTimeOffset.UtcNow.AddHours(-1))
                {
                    return Tweet.CreateAt.ToLocalTime().ToString("H:mm");
                }

                // H:mm:ss
                return Tweet.CreateAt.ToLocalTime().ToString("H:mm:ss");
            }
        }

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

        private void OnClickAvatar(MouseEventArgs _) => NavigateToProfilePage();

        private void OnClickUserDisplayName(MouseEventArgs _) => NavigateToProfilePage();

        private void OnClickUserDisplayId(MouseEventArgs _) => NavigateToProfilePage();

        private void NavigateToProfilePage()
        {
            if (Tweet == null)
            {
                return;
            }

            var url = string.Format(DefinePaths.PAGE_PATH_PROFILE, Tweet.UserDisplayId);
            Navigation.NavigateTo(url);
        }

        private void OnClickDeleteButton(MouseEventArgs _)
        {
            OnDeleteAction?.Invoke(Tweet!.Id);
        }


        private void OnClickReplyButton(MouseEventArgs _)
        {
            IsOpendReplyPostForm = true;
        }

        private void OnClickReplyPostCloseButton(MouseEventArgs _)
        {
            IsOpendReplyPostForm = false;
        }
    }
}
