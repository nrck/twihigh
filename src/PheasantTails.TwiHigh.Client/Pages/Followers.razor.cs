﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Followers
    {
        [Parameter]
        public string Id { get; set; } = string.Empty;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private FollowHttpClient FollowHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private AppUserHttpClient AppUserHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private ILocalStorageService LocalStorage { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationState { get; set; }

        private ResponseTwiHighUserContext? User { get; set; }

        private ResponseTwiHighUserContext[]? UserFollowers { get; set; }
        private string Title { get; set; } = "プロフィール読み込み中";

        protected override async Task OnInitializedAsync()
        {
            User = await AppUserHttpClient.GetTwiHighUserAsync(Id);
            if (User == null)
            {
                Title = "プロフィールを読み込めませんでした。";
            }
            else
            {
                Title = $"{User.DisplayName}（@{User.DisplayId}）のフォロワー";
            }
            UserFollowers = await AppUserHttpClient.GetTwiHighUserFollowersAsync(Id);
            StateHasChanged();
            await base.OnInitializedAsync();
        }
    }
}
