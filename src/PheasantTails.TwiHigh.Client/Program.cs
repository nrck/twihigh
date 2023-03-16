using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PheasantTails.TwiHigh.Client.TypedHttpClients;

namespace PheasantTails.TwiHigh.Client
{
    public class Program
    {
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

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Local", options.ProviderOptions);
            });
            await builder.Build().RunAsync();
        }
    }
}