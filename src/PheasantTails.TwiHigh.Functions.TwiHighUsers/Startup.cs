using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Core.Services;
using PheasantTails.TwiHigh.Functions.TwiHighUsers;

[assembly: FunctionsStartup(typeof(Startup))]
namespace PheasantTails.TwiHigh.Functions.TwiHighUsers
{
    public class Startup : TwiHighFunctionStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            base.Configure(builder);
            builder.Services.AddOptions<AzureBlobStorageServiceOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.Bind("AzureBlobStorageService", options));
            builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
            builder.Services.AddScoped<IImageProcesserService, ImageProcesserService>();
        }
    }
}
