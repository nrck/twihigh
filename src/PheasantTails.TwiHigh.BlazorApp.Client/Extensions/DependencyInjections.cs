﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.TypedHttpClients;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

internal static class DependencyInjections
{
    internal static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<IHomeViewModel, HomeViewModel>();
        services.AddTransient<IIndexViewModel, IndexViewModel>();
        services.AddTransient<ILoginViewModel, LoginViewModel>();

        return services;
    }

    internal static IServiceCollection AddTwiHighservices(this IServiceCollection services)
    {
        services.AddSingleton<ITimelineWorkerService, TimelineWorkerService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();

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
        services.AddBlazoredLocalStorageAsSingleton();

        return services;
    }

    internal static IServiceCollection SetupTwiHighWebAssembly(this IServiceCollection services)
    {
        services.AddTwiHighMiddleware();
        services.AddViewModels();
        services.AddTwiHighservices();
        services.AddTwiHighApiClient();

        return services;
    }
}
