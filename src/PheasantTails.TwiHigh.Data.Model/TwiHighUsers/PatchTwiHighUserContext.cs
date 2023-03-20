namespace PheasantTails.TwiHigh.Data.Model.TwiHighUsers
{
    public class PatchTwiHighUserContext
    {
        public string? DisplayId { get; set; }
        public string? DisplayName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public Base64EncodedFileContent? Base64EncodedAvatarImage { get; set; }
        public string? Biography { get; set; }

        public byte[]? DecodeAvaterImage()
        {
            if (string.IsNullOrEmpty(Base64EncodedAvatarImage?.Data))
            {
                return null;
            }

            return Convert.FromBase64String(Base64EncodedAvatarImage.Data);
        }

        public void EncodeAvaterImage(string contentType, byte[] data)
        {
            var file = new Base64EncodedFileContent
            {
                ContentType = contentType,
                Data = Convert.ToBase64String(data)
            };
            Base64EncodedAvatarImage = file;
        }

        public class Base64EncodedFileContent
        {
            public string ContentType { get; set; } = string.Empty;
            public string Data { get; set; } = string.Empty;
        }
    }
}
