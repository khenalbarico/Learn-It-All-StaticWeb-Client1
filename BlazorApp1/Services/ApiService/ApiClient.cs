using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BlazorApp1.Models;
using BlazorApp1.Services.AuthService;

namespace BlazorApp1.Services.ApiService;

public class ApiClient(HttpClient _http, IAppAuthentication _auth) : IApiClient
{
    private async Task<HttpRequestMessage> BuildRequestAsync(ApiFunctions apiFunction, object? payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ApiRouting.Routes[apiFunction]);

        if (!ApiRouting.Anonymous.Contains(apiFunction))
        {
            var auth = await _auth.GetAuthAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
            request.Headers.Add("X-Uid", auth.Uid);
        }

        if (payload is not null)
            request.Content = JsonContent.Create(payload);

        return request;
    }

    private async Task<HttpResponseMessage> SendOnceAsync(ApiFunctions apiFunction, object? payload)
    {
        try
        {
            return await _http.SendAsync(await BuildRequestAsync(apiFunction, payload));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiUnavailableException(
                "Could not reach the Learn It All server. Please check your internet connection and try again.",
                ex);
        }
    }

    private async Task<HttpResponseMessage> SendAsync(ApiFunctions apiFunction, object? payload)
    {
        var response = await SendOnceAsync(apiFunction, payload);

        // A 401 means the ID token lapsed between build and send; GetAuthAsync will
        // force-refresh on the retry because the cached expiry has now passed.
        if (response.StatusCode == HttpStatusCode.Unauthorized && !ApiRouting.Anonymous.Contains(apiFunction))
            response = await SendOnceAsync(apiFunction, payload);

        return response;
    }

    public async Task<T> GetAsync<T>(ApiFunctions apiFunction, object? payload = null)
    {
        var response = await SendAsync(apiFunction, payload);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public Task<T> SubmitAsync<T>(ApiFunctions apiFunction, object? payload = null)
        => GetAsync<T>(apiFunction, payload);

    public async Task SubmitAsync(ApiFunctions apiFunction, object? payload = null)
    {
        var response = await SendAsync(apiFunction, payload);
        response.EnsureSuccessStatusCode();
    }
}
