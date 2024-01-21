using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.TypedHttpClients;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

namespace PheasantTails.TwiHigh.BlazorApp.Extensions;

public static class DependencyInjections
{
    internal static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddScoped<IHomeViewModel, HomeViewModel>();
        services.AddScoped<IIndexViewModel, ViewModels.IndexViewModel>();
        services.AddScoped<ILoginViewModel, LoginViewModel>();

        return services;
    }

    internal static IServiceCollection AddTwiHighservices(this IServiceCollection services)
    {
        services.AddScoped<ITimelineWorkerService, TimelineWorkerService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();

        return services;
    }

    internal static IServiceCollection AddTwiHighApiClient(this IServiceCollection services)
    {
        services.AddHttpClient<TimelineHttpClient>();
        services.AddHttpClient<AppUserHttpClient>();
        services.AddHttpClient<TweetHttpClient>();
        services.AddHttpClient<FollowHttpClient>();
        services.AddHttpClient<FeedHttpClient>();

        return services;
    }

    internal static IServiceCollection AddTwiHighMiddleware(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();

        return services;
    }

    internal static IServiceCollection SetupTwiHighServer(this IServiceCollection services)
    {
        services.AddTwiHighMiddleware();
        services.AddViewModels();
        services.AddTwiHighservices();
        services.AddTwiHighApiClient();

        return services;
    }
}
