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
                return;
            }

            if (1 * 1024 * 1024 < file.Size)
            {
                return;
            }
            byte[] data = new byte[file.Size];
            var stream = file.OpenReadStream(1 * 1024 * 1024);
            await stream.ReadAsync(data);

            var base64string = Convert.ToBase64String(data);
            AvatarUrl = $"data:{file.ContentType};base64,{base64string}";
            PatchContext.EncodeAvaterImage(file.ContentType, data);
            StateHasChanged();
        }

        private async Task OnClickSaveButtonAsync()
        {
            AdjustPatchContext();
            // 送信する
            User = await AppUserHttpClient.PatchTwiHighUserAsync(PatchContext);
            SetDisplayVariables();
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
        }

        private void OnClickAvatarResetButton()
        {
            PatchContext.Base64EncodedAvatarImage = null;
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
