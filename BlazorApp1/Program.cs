using BlazorApp1;
using BlazorApp1.Services;
using BlazorApp1.Services.App;
using BlazorApp1.Services.Theme;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var apiEnvironment = builder.Configuration["ApiEnvironment"] ?? "Dev";
builder.Services.AddLearnItAllServices(apiEnvironment);

var host = builder.Build();

await host.Services.GetRequiredService<ThemeService>().InitializeAsync();
await host.Services.GetRequiredService<AuthSessionState>().FastInitializeAsync();

await host.RunAsync();
