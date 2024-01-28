using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using Reactive.Bindings;
using System.Windows.Input;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Components
{
    public partial class THPostForm : TwiHighComponentBase
    {
        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;

        [Parameter]
        public ReplyToContext? ReplyToContext { get; set; }

        [Parameter]
        public ICommand OnClickAvatarCommand { get; set; } = default!;

        [Parameter, EditorRequired]
        public AsyncReactiveCommand<PostTweetContext> PostTweetCommand { get; set; } = default!;

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
            await PostTweetCommand.ExecuteAsync(PostTweetContext).ConfigureAwait(false);
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

        private void OnClickMyAvatar() => OnClickAvatarCommand.Execute(null);
    }
}
