using BlazorApp1;
using BlazorApp1.Services;
using BlazorApp1.Services.App;
using BlazorApp1.Services.AuthService;
using BlazorApp1.Services.Theme;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var browserHost = new Uri(builder.HostEnvironment.BaseAddress).Host;
var apiEnvironment = browserHost switch
{
    "localhost"                                   => "localhost",
    "gentle-hill-0630fc410.7.azurestaticapps.net" => "dev",
    "gentle-wave-04a543e10.7.azurestaticapps.net" => "prod",
    _                                             => "prod"
};

var firebaseCfg = builder.Configuration.GetSection("Firebase").Get<FirebaseCfg>() ?? new FirebaseCfg();
builder.Services.AddLearnItAllServices(apiEnvironment, firebaseCfg);

var host = builder.Build();

await host.Services.GetRequiredService<ThemeService>().InitializeAsync();
await host.Services.GetRequiredService<AuthSessionState>().InitializeAsync();

Console.WriteLine($"[Learn It All] Initialized - {apiEnvironment} environment");

await host.RunAsync();
