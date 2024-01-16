using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.TypedHttpClients;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

public static class DependencyInjections
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<HomeViewModel>();
        services.AddTransient<IndexViewModel>();
        services.AddTransient<LoginViewModel>();
        return services;
    }

    public static IServiceCollection AddTwiHighservices(this IServiceCollection services)
    {
        services.AddSingleton<ITimelineWorkerService, TimelineWorkerService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();
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
