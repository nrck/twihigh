﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using PheasantTails.TwiHigh.Client.Pages;
using PheasantTails.TwiHigh.Client.Services;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

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

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private IMessageService MessageService { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;

        [Parameter]
        public ReplyToContext? ReplyToContext { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs>? OnReplyTweetPosted { get; set; }

        [CascadingParameter]
        protected Task<AuthenticationState>? AuthenticationState { get; set; }

        private PostTweetContext PostTweetContext { get; set; } = new PostTweetContext();

        private bool IsPosting { get; set; }

        private string TweetText { get; set; } = string.Empty;


        protected override async Task OnInitializedAsync()
        {
            var token = await LocalStorage.GetItemAsStringAsync("TwiHighJwt");
            if (AuthenticationState != null)
            {
                UserAvatarUrl = (await AuthenticationState).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.AvatarUrl))?.Value ?? string.Empty;
            }
            TweetHttpClient.SetToken(token);
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
            var res = await TweetHttpClient.PostTweetAsync(PostTweetContext);
            if (res != null && res.IsSuccessStatusCode)
            {
                MessageService.Set(MessageComponent.MessageLevel.Success, "ツイートを送信しました！");
            }
            else
            {
                MessageService.Set(MessageComponent.MessageLevel.Error, "ツイートできませんでした。");
            }
            PostTweetContext.Text = string.Empty;
            PostTweetContext.ReplyTo = null;
            TweetText = string.Empty;
            IsPosting = false;
            if(ReplyToContext != null && OnReplyTweetPosted != null)
            {
                await OnReplyTweetPosted.Value.InvokeAsync();
            }
            StateHasChanged();
        }

        private async Task OnKeyPressAsync(KeyboardEventArgs e)
        {
            if (e.CtrlKey && e.Code == "Enter" && !IsPosting)
            {
                await OnSubmitAsync();
                StateHasChanged();
            }
        }

        private void OnInputTextarea(ChangeEventArgs e)
        {
            TweetText = e.Value?.ToString() ?? string.Empty;
        }

        private void OnClickMyAvatar()
        {
            NavigationManager.NavigateTo(DefinePaths.PAGE_PATH_PROFILE_EDITOR);
        }
    }
}
