using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PheasantTails.TwiHigh.Functions.Core.Extensions;
using System;

IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(Environment.CurrentDirectory)
        .AddEnvironmentVariables()
        .Build();

IHost host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureTwiHighFunctions()
    .Build();

host.Run();
