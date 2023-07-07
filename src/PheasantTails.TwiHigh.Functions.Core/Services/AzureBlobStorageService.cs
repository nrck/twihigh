using Azure.Storage.Blobs;

namespace PheasantTails.TwiHigh.Functions.Core.Services
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<Uri> UploadAsync(string containerName, string blobName, BinaryData content)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content);

            return blobClient.Uri;
        }
    }
}
