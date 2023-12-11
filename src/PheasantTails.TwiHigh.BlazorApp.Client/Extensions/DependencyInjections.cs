using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

internal static class DependencyInjections
{
    internal static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<HomeViewModel>();
        return services;
    }

    internal static IServiceCollection AddTwiHighservices(this IServiceCollection services)
    {
        services.AddSingleton<ITimelineWorkerService, TimelineWorkerService>();
        return services;
    }
}
