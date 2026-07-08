namespace BlazorApp1.Services.ApiService;

public class ApiUrlGetter : IApiUrlGetter
{
    public string GetApiUrl(string env)
    {
        env = env.Trim().ToLowerInvariant();

        return env switch
        {
            //"dev"       => "http://localhost:7041/api/",
            "dev"       => "https://learnitallapidev1.azurewebsites.net/api/",
            "prod"      => "https://learnitallapiprod1.azurewebsites.net/api/", 
            _           => throw new ArgumentException($"Unknown environment: {env}")
        };
    }
}
