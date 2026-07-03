using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BlazorApp1.Models;
using BlazorApp1.Services.AuthService;

namespace BlazorApp1.Services.ApiService;

public class ApiClient(HttpClient _http, TokenCache _cache, IAppAuthentication _auth) : IApiClient
{
    private async Task<AuthResult> ResolveAuthAsync()
    {
        if (_cache.TryGet(out var cached) && cached is not null)
            return cached;

        return await _auth.RefreshAsync();
    }

    private async Task<HttpRequestMessage> BuildRequestAsync(ApiFunctions apiFunction, object? payload = null)
    {
        var result  = await ResolveAuthAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, ApiRouting.Routes[apiFunction]);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        request.Headers.Add("X-Uid", result.Uid);

        if (payload is not null)
            request.Content = JsonContent.Create(payload);

        return request;
    }

    private async Task<HttpResponseMessage> SendOnceAsync(Func<Task<HttpRequestMessage>> buildRequest)
    {
        try
        {
            return await _http.SendAsync(await buildRequest());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiUnavailableException(
                "Could not reach the Learn It All server. Please check your internet connection and try again.",
                ex);
        }
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(Func<Task<HttpRequestMessage>> buildRequest)
    {
        var response = await SendOnceAsync(buildRequest);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _cache.Clear();
            response = await SendOnceAsync(buildRequest);
        }

        return response;
    }

    public async Task<T> GetAsync<T>(ApiFunctions apiFunction, object? payload = null)
    {
        var response = await SendWithRetryAsync(() => BuildRequestAsync(apiFunction, payload));
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task<T> SubmitAsync<T>(ApiFunctions apiFunction, object? payload = null)
    {
        var response = await SendWithRetryAsync(() => BuildRequestAsync(apiFunction, payload));
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task SubmitAsync(ApiFunctions apiFunction, object? payload = null)
    {
        var response = await SendWithRetryAsync(() => BuildRequestAsync(apiFunction, payload));
        response.EnsureSuccessStatusCode();
    }

    public async Task<byte[]> GetBytesAsync(ApiFunctions apiFunction, object? payload = null)
    {
        var response = await SendWithRetryAsync(() => BuildRequestAsync(apiFunction, payload));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}
