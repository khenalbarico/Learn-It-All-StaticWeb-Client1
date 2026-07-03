namespace BlazorApp1.Services.ApiService;

public interface IApiUrlGetter
{
    string GetApiUrl(string env);
}
