using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace PheasantTails.TwiHigh.BlazorApp.Extensions;

public static class DependencyInjections
{
    internal static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddScoped<IFeedsViewModel, FeedsViewModel>();
        services.AddScoped<IFollowersViewModel, FollowsAndFollowersViewModel>();
        services.AddScoped<IFollowsViewModel, FollowsAndFollowersViewModel>();
        services.AddScoped<IHomeViewModel, HomeViewModel>();
        services.AddScoped<ILoginViewModel, LoginViewModel>();
        services.AddScoped<IProfileEditerViewModel, ProfileEditerViewModel>();
        services.AddScoped<IProfileViewModel, ProfileViewModel>();
        services.AddScoped<ISignupViewModel, SignupViewModel>();
        services.AddScoped<IStatusViewModel, StatusViewModel>();

        return services;
    }

    internal static IServiceCollection AddTwiHighservices(this IServiceCollection services)
    {
        services.AddScoped<AuthenticationStateProvider, TwiHighServerAuthenticationStateProvider>();
        services.AddScoped<IFeedWorkerService, FeedWorkerService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IScrollInfoService, ScrollInfoService>();
        services.AddScoped<ITimelineWorkerService, TimelineWorkerService>();

        return services;
    }

    internal static IServiceCollection AddTwiHighMiddleware(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddPWAUpdater();
        services.AddHttpClient();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();
        services.AddCascadingAuthenticationState();

        return services;
    }

    internal static IServiceCollection SetupTwiHighServer(this IServiceCollection services)
    {
        services.AddTwiHighMiddleware();
        services.AddViewModels();
        services.AddTwiHighservices();

        return services;
    }
}
