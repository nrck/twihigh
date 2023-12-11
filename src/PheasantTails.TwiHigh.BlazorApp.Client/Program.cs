using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddTwiHighservices();
builder.Services.AddViewModels();

await builder.Build().RunAsync();
