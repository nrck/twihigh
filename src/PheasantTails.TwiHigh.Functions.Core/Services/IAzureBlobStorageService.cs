namespace PheasantTails.TwiHigh.Functions.Core.Services
{
    public interface IAzureBlobStorageService
    {
        Task<Uri> UploadAsync(string containerName, string blobName, BinaryData content);
    }
}