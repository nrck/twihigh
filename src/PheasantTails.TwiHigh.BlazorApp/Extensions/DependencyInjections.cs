using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.TypedHttpClients;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

namespace PheasantTails.TwiHigh.BlazorApp.Extensions;

public static class DependencyInjections
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddScoped<HomeViewModel>();
        services.AddScoped<IndexViewModel>();
        services.AddScoped<LoginViewModel>();
        return services;
    }

    public static IServiceCollection AddTwiHighservices(this IServiceCollection services)
    {
        services.AddScoped<ITimelineWorkerService, TimelineWorkerService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();
        return services;
    }

    public static IServiceCollection AddTwiHighApiClient(this IServiceCollection services)
    {
        services.AddHttpClient<TimelineHttpClient>();
        services.AddHttpClient<AppUserHttpClient>();
        services.AddHttpClient<TweetHttpClient>();
        services.AddHttpClient<FollowHttpClient>();
        services.AddHttpClient<FeedHttpClient>();
        return services;
    }
}
