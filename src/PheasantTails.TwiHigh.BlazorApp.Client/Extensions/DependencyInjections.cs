using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

internal static class DependencyInjections
{
    internal static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<IFeedsViewModel, FeedsViewModel>();
        services.AddTransient<IFollowersViewModel, FollowsAndFollowersViewModel>();
        services.AddTransient<IFollowsViewModel, FollowsAndFollowersViewModel>();
        services.AddTransient<IHomeViewModel, HomeViewModel>();
        services.AddTransient<ILoginViewModel, LoginViewModel>();
        services.AddTransient<IProfileEditerViewModel, ProfileEditerViewModel>();
        services.AddTransient<IProfileViewModel, ProfileViewModel>();
        services.AddTransient<ISignupViewModel, SignupViewModel>();
        services.AddTransient<IStatusViewModel, StatusViewModel>();

        return services;
    }

    internal static IServiceCollection AddTwiHighservices(this IServiceCollection services)
    {
        services.AddSingleton<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();
        services.AddSingleton<IFeedWorkerService, FeedWorkerService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<IScrollInfoService, ScrollInfoService>();
        services.AddSingleton<ITimelineWorkerService, TimelineWorkerService>();

        return services;
    }

    internal static IServiceCollection AddTwiHighMiddleware(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorageAsSingleton();
        services.AddPWAUpdater();
        services.AddHttpClient();
        services.AddCascadingAuthenticationState();
        services.AddOptions();
        services.AddAuthorizationCore();

        return services;
    }

    internal static IServiceCollection SetupTwiHighWebAssembly(this IServiceCollection services)
    {
        services.AddTwiHighMiddleware();
        services.AddViewModels();
        services.AddTwiHighservices();

        return services;
    }
}
