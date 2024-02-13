using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
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
        services.AddScoped<IIndexViewModel, ViewModels.IndexViewModel>();
        services.AddScoped<ILoginViewModel, LoginViewModel>();
        services.AddScoped<IProfileEditerViewModel, ProfileEditerViewModel>();
        services.AddScoped<IProfileViewModel, ProfileViewModel>();
        services.AddScoped<ISignupViewModel, SignupViewModel>();
        services.AddScoped<IStatusViewModel, StatusViewModel>();

        return services;
    }

    internal static IServiceCollection AddTwiHighservices(this IServiceCollection services)
    {
        services.AddScoped<AuthenticationStateProvider, TwiHighAuthenticationStateProvider>();
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
        services.AddAuthentication()
            .AddCookie(option =>
            {
                option.LoginPath = new PathString("/");
                option.ReturnUrlParameter = "";
            });
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
