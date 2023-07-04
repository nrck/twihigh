using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace PheasantTails.TwiHigh.Functions.Core.Services
{
    public class AzureBlobStorageService: IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        
        public AzureBlobStorageService(IOptions<AzureBlobStorageServiceOptions> options)
        {
            _blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
        }

        public async Task<Uri> UploadAsync(string containerName,string blobName, BinaryData content)
        {
            BlobContainerClient containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content);

            return blobClient.Uri;
        }
    }

    public class AzureBlobStorageServiceOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
