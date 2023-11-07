using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.Beta.Client;
using PheasantTails.TwiHigh.Beta.Client.Pages;
using PheasantTails.TwiHigh.Beta.Client.Services;
using PheasantTails.TwiHigh.Beta.Client.TypedHttpClients;
using PheasantTails.TwiHigh.Beta.Server.Components;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace PheasantTails.TwiHigh.Beta.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            // Œ^•t‚«HttpClient
            builder.Services.AddHttpClient<TimelineHttpClient>();
            builder.Services.AddHttpClient<AppUserHttpClient>();
            builder.Services.AddHttpClient<TweetHttpClient>();
            builder.Services.AddHttpClient<FollowHttpClient>();
            builder.Services.AddHttpClient<FeedHttpClient>();

            // ƒ~ƒhƒ‹
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();
            builder.Services.AddPWAUpdater();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IScrollInfoService, ScrollInfoService>();
            builder.Services.AddScoped<IFeedService, FeedService>();

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Local", options.ProviderOptions);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthorization();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Counter).Assembly);

            app.Run();
        }
    }
}
