using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Core.Services;
using PheasantTails.TwiHigh.Functions.TwiHighUsers;
using System;

[assembly: FunctionsStartup(typeof(Startup))]
namespace PheasantTails.TwiHigh.Functions.TwiHighUsers
{
    public class Startup : TwiHighFunctionStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            base.Configure(builder);
            builder.Services.AddSingleton((s) =>
            {
                var connectionString = configuration["BlobStorageConnectionString"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException("BlobStorageConnectionString", "Azure Functionsの設定値「BlobStorageConnectionString」が未設定です。");
                }

                return new BlobServiceClient(connectionString);
            });
            builder.Services.AddSingleton<IAzureBlobStorageService, AzureBlobStorageService>();
            builder.Services.AddSingleton<IImageProcesserService, ImageProcesserService>();
        }
    }
}
