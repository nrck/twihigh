using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddTwiHighApiClient();
builder.Services.AddTwiHighservices();
builder.Services.AddViewModels();

await builder.Build().RunAsync();
