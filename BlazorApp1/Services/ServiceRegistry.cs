using BlazorApp1.Services.ApiService;
using BlazorApp1.Services.App;
using BlazorApp1.Services.AuthService;
using BlazorApp1.Services.Caching;
using BlazorApp1.Services.Theme;

namespace BlazorApp1.Services;

public static class ServiceRegistry
{
    public static IServiceCollection AddLearnItAllServices(this IServiceCollection services, string apiEnvironment)
    {
        services.AddSingleton<IAdSenseCfg, AdSenseWebCfg>();
        services.AddSingleton<JsInteropService>();
        services.AddSingleton<IAppAuthentication, EntraAuthentication>();

        services.AddSingleton<IApiUrlGetter, ApiUrlGetter>();
        services.AddHttpClient("LearnItAllApi", (sp, client) =>
        {
            var apiUrlGetter = sp.GetRequiredService<IApiUrlGetter>();
            var apiUrl       = apiUrlGetter.GetApiUrl(apiEnvironment);

            client.BaseAddress = new Uri(apiUrl);
            client.Timeout     = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddSingleton<IApiClient>(sp => new ApiClient(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("LearnItAllApi"),
            sp.GetRequiredService<IAppAuthentication>()));

        services.AddSingleton<IAppService, AppService>();
        services.AddSingleton<IPaymentService, PaymentService>();
        services.AddSingleton<LibraryCacheService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<AuthSessionState>();

        return services;
    }
}
