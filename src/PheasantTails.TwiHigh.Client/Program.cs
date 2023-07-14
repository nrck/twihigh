using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PheasantTails.TwiHigh.Client.Services;
using PheasantTails.TwiHigh.Client.TypedHttpClients;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace PheasantTails.TwiHigh.Client
{
    public class Program
    {
        public static string TwiHighVersion => "0.8.7";

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Œ^•t‚«HttpClient
            builder.Services.AddHttpClient<TimelineHttpClient>();
            builder.Services.AddHttpClient<AppUserHttpClient>();
            builder.Services.AddHttpClient<TweetHttpClient>();
            builder.Services.AddHttpClient<FollowHttpClient>();

            // ƒ~ƒhƒ‹
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();
            builder.Services.AddPWAUpdater();
            builder.Services.AddSingleton<IMessageService, MessageService>();
            builder.Services.AddSingleton<IScrollInfoService, ScrollInfoService>();

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Local", options.ProviderOptions);
            });

#if DEBUG
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

            await builder.Build().RunAsync();
        }
    }
}