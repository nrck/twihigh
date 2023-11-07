using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Beta.Client.Components
{
    public partial class PostFormComponent : UIComponentBase
    {
        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;

        [Parameter]
        public ReplyToContext? ReplyToContext { get; set; }

        [Parameter]
        public EventCallback OnClickAvatar { get; set; }

        [Parameter]
        public EventCallback<PostTweetContext> OnPostTweet { get; set; }

        [Parameter]
        public string TweetText { get; set; } = string.Empty;

        [Parameter]
        public bool IsForceForcus { get; set; }

        private PostTweetContext PostTweetContext { get; set; } = new PostTweetContext();

        private bool IsPosting { get; set; }

        private ElementReference TextArea { get; set; }

        public ValueTask TextAreaFocusAsync() => TextArea.FocusAsync();

        protected override async Task OnInitializedAsync()
        {
            if (AuthenticationState != null)
            {
                UserAvatarUrl = (await AuthenticationState).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.AvatarUrl))?.Value ?? string.Empty;
            }
            await base.OnInitializedAsync();
        }

        private async Task OnSubmitAsync()
        {
            if (string.IsNullOrEmpty(TweetText) || IsPosting)
            {
                return;
            }

            IsPosting = true;
            StateHasChanged();
            PostTweetContext.Text = TweetText;
            PostTweetContext.ReplyTo = ReplyToContext;
            await OnPostTweet.InvokeAsync(PostTweetContext);
            PostTweetContext.Text = string.Empty;
            PostTweetContext.ReplyTo = null;
            TweetText = string.Empty;
            IsPosting = false;
            if (IsForceForcus)
            {
                StateHasChanged();
                await TextArea.FocusAsync();
            }
        }

        private async Task OnKeyPressAsync(KeyboardEventArgs e)
        {
            if (IsPosting)
            {
                // 処理中なら送信しない
                return;
            }

            if (e.CtrlKey && (e.Code == "Enter" || e.Code == "NumpadEnter"))
            {
                await OnSubmitAsync();
            }
        }

        private void OnInputTextarea(ChangeEventArgs e) => TweetText = e.Value?.ToString() ?? string.Empty;

        private void OnClickMyAvatar() => OnClickAvatar.InvokeAsync();
    }
}
