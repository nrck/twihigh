﻿using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Follows : PageBase
    {
        [Parameter]
        public string Id { get; set; } = string.Empty;

        private ResponseTwiHighUserContext? User { get; set; }

        private ResponseTwiHighUserContext[]? UserFollowers { get; set; }
        private string Title { get; set; } = "プロフィール読み込み中";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            User = await AppUserHttpClient.GetTwiHighUserAsync(Id);
            if (User == null)
            {
                Title = "プロフィールを読み込めませんでした。";
            }
            else
            {
                Title = $"{User.DisplayName}（@{User.DisplayId}）のフォロー中";
            }
            UserFollowers = await AppUserHttpClient.GetTwiHighUserFollowsAsync(Id);
            StateHasChanged();
        }
    }
}
