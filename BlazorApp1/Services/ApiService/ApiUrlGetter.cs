namespace BlazorApp1.Services.ApiService;

public class ApiUrlGetter : IApiUrlGetter
{
    public string GetApiUrl(string env)
    {
        env = env.Trim().ToLowerInvariant();

        return env switch
        {
            "localhost" => "http://localhost:7041/api/",
            "dev"       => "https://learn-it-all-api-dev1.azurewebsites.net/api/",
            "prod"      => "https://REPLACE_WITH_PROD_URL.azurewebsites.net/api/", // no Prod Function App exists yet - update once deployed
            _           => throw new ArgumentException($"Unknown environment: {env}")
        };
    }
}
