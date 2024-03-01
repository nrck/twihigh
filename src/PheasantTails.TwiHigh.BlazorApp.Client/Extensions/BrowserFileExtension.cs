using Microsoft.AspNetCore.Components.Forms;
using static PheasantTails.TwiHigh.Data.Model.TwiHighUsers.PatchTwiHighUserContext;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

public static class BrowserFileExtension
{
    public static bool IsSupportedImage(this IBrowserFile file)
        => file.ContentType is "image/png" or "image/jpeg";

    public static bool IsEmpty(this IBrowserFile file)
        => file.Size <= 0;

    public static bool IsSupportedMaximumSize(this IBrowserFile file)
        => (file.Size <= 5 * 1024 * 1024) && file.IsEmpty() == false;

    public static async Task<Base64EncodedFileContent> ToBase64EncodedFileContentAsync(this IBrowserFile file)
    {
        byte[] data = new byte[file.Size];
        using Stream stream = file.OpenReadStream();
        await stream.ReadAsync(data);
        return new Base64EncodedFileContent
        {
            ContentType = file.ContentType,
            Data = Convert.ToBase64String(data)
        };
    }
}
