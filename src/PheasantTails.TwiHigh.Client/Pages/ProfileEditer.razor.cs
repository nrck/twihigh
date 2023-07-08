using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class ProfileEditer : PageBase
    {
        private ResponseTwiHighUserContext? User { get; set; }

        private string Title { get; set; } = "プロフィール読み込み中";

        private string AvatarUrl { get; set; } = string.Empty;

        private PatchTwiHighUserContext PatchContext { get; set; } = new PatchTwiHighUserContext();
        private string DisplayId { get; set; } = string.Empty;
        private string DisplayName { get; set; } = string.Empty;
        private string Biography { get; set; } = string.Empty;
        private byte[] LocalRowAvatarData { get; set; } = Array.Empty<byte>();
        private string LocalRowAvatarContentType { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var id = (await AuthenticationState!).User.Claims.FirstOrDefault(c => c.Type == nameof(ResponseTwiHighUserContext.Id))?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(id))
            {
                await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOutAsync();
                Navigation.NavigateTo(DefinePaths.PAGE_PATH_LOGIN);
            }

            User = await AppUserHttpClient.GetTwiHighUserAsync(id);
            if (User == null)
            {
                await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOutAsync();
                Navigation.NavigateTo(DefinePaths.PAGE_PATH_LOGIN);
                return;
            }
            SetDisplayVariables();
            StateHasChanged();
        }

        private async Task LoadFiles(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (
                file.ContentType != "image/png" &&
                file.ContentType != "image/jpeg"
            )
            {
                SetWarnMessage("画像はPNGもしくはJPEGのみがサポートされています。他の画像を使用してください。");
                return;
            }

            if (file.Size <= 0)
            {
                SetErrorMessage("画像データを読み込めませんでした。");
                return;
            }

            if (1 * 1024 * 1024 < file.Size)
            {
                SetWarnMessage("画像の最大サイズは5MBです。リサイズするなど、ファイルサイズを小さくしてください。");
                return;
            }
            LocalRowAvatarData = new byte[file.Size];
            LocalRowAvatarContentType = file.ContentType;
            var stream = file.OpenReadStream(5 * 1024 * 1024);
            await stream.ReadAsync(LocalRowAvatarData);

            var base64string = Convert.ToBase64String(LocalRowAvatarData);
            AvatarUrl = $"data:{LocalRowAvatarContentType};base64,{base64string}";
            StateHasChanged();
        }

        private async Task OnClickSaveButtonAsync()
        {
            AdjustPatchContext();
            // 送信する
            var tmp = await AppUserHttpClient.PatchTwiHighUserAsync(PatchContext);
            if (tmp == null)
            {
                SetErrorMessage("プロフィールの更新に失敗しました。");
            }
            else
            {
                SetSucessMessage("プロフィールを更新しました！");
                await ((TwiHighAuthenticationStateProvider)AuthenticationStateProvider).RefreshAuthenticationStateAsync();
                User = tmp;
                SetDisplayVariables();
            }
            StateHasChanged();
        }

        private void AdjustPatchContext()
        {
            if (DisplayName != User?.DisplayName)
            {
                PatchContext.DisplayName = DisplayName;
            }
            if (DisplayId != User?.DisplayId)
            {
                PatchContext.DisplayId = DisplayId;
            }
            if (Biography != User?.Biography)
            {
                PatchContext.Biography = Biography;
            }
            if (0 < LocalRowAvatarData.LongLength && !string.IsNullOrEmpty(LocalRowAvatarContentType))
            {
                PatchContext.EncodeAvaterImage(LocalRowAvatarContentType, LocalRowAvatarData);
            }
        }

        private void OnClickAvatarResetButton()
        {
            LocalRowAvatarContentType = string.Empty;
            LocalRowAvatarData = Array.Empty<byte>();
            AvatarUrl = User!.AvatarUrl;
        }

        private void SetDisplayVariables()
        {
            Title = $"{User!.DisplayName}（@{User.DisplayId}）";
            AvatarUrl = User.AvatarUrl;
            DisplayId = User.DisplayId;
            DisplayName = User.DisplayName;
            Biography = User.Biography;
        }
    }
}
