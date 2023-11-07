using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PheasantTails.TwiHigh.Beta.Client.Services;
using PheasantTails.TwiHigh.Beta.Client.TypedHttpClients;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace PheasantTails.TwiHigh.Beta.Client;

public class Program
{
    public static string TwiHighVersion => "0.9.6";

    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Œ^•t‚«HttpClient
        builder.Services.AddHttpClient<TimelineHttpClient>();
        builder.Services.AddHttpClient<AppUserHttpClient>();
        builder.Services.AddHttpClient<TweetHttpClient>();
        builder.Services.AddHttpClient<FollowHttpClient>();
        builder.Services.AddHttpClient<FeedHttpClient>();

        // ƒ~ƒhƒ‹
        builder.Services.AddBlazoredLocalStorageAsSingleton();
        builder.Services.AddSingleton<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();
        builder.Services.AddPWAUpdater();
        builder.Services.AddSingleton<IMessageService, MessageService>();
        builder.Services.AddSingleton<IScrollInfoService, ScrollInfoService>();
        builder.Services.AddSingleton<IFeedService, FeedService>();

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