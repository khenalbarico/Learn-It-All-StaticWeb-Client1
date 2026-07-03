namespace BlazorApp1.Services.ApiService;

public class ApiUrlGetter : IApiUrlGetter
{
    public string GetApiUrl(string env)
    {
        env = env.Trim().ToLowerInvariant();

        return env switch
        {
            "development" => "https://learn-it-all-api-dev1.azurewebsites.net/api/",
            "production"  => "https://learn-it-all-api-dev1.azurewebsites.net/api/",
            _             => throw new ArgumentException($"Unknown environment: {env}")
        };
    }
}
