using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    // The API answers with a deliberate status per failure mode; EnsureSuccessStatusCode
    // would flatten all of them into one opaque HttpRequestException and the UI could only
    // ever guess. Translate them back into something callers can act on.
    private static async Task ThrowIfFailedAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();

        throw response.StatusCode switch
        {
            HttpStatusCode.Conflict           => new AlreadyOwnedException(Describe(body, "You already own this book.")),
            HttpStatusCode.TooManyRequests    => new TooManyAttemptsException(Describe(body, "Too many attempts. Please wait a moment."), RetryAfterFrom(body)),
            HttpStatusCode.BadGateway         => new PaymentProviderUnavailableException(Describe(body, "The payment provider is unavailable. Please try again shortly.")),
            // The status must be carried on the exception, not just in its text: TryGetUserInfo
            // filters on StatusCode == NotFound to detect a first-time user, and a null there
            // silently turns "no profile yet" into a hard error that hides the setup form.
            _                                 => new HttpRequestException($"{(int)response.StatusCode}: {body}", null, response.StatusCode)
        };
    }

    // Errors arrive either as a bare string or as { message, retryAfterSeconds }.
    private static string Describe(string body, string fallback)
    {
        if (string.IsNullOrWhiteSpace(body)) return fallback;

        try   { return JsonDocument.Parse(body).RootElement.TryGetProperty("message", out var m) ? m.GetString() ?? fallback : body.Trim('"'); }
        catch { return body.Trim('"'); }
    }

    private static int RetryAfterFrom(string body)
    {
        try   { return JsonDocument.Parse(body).RootElement.TryGetProperty("retryAfterSeconds", out var s) ? s.GetInt32() : 0; }
        catch { return 0; }
    }

    public async Task<T> GetAsync<T>(ApiFunctions apiFunction, object? payload = null)
    {
        var response = await SendAsync(apiFunction, payload);
        await ThrowIfFailedAsync(response);

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public Task<T> SubmitAsync<T>(ApiFunctions apiFunction, object? payload = null)
        => GetAsync<T>(apiFunction, payload);

    public async Task SubmitAsync(ApiFunctions apiFunction, object? payload = null)
    {
        var response = await SendAsync(apiFunction, payload);
        await ThrowIfFailedAsync(response);
    }
}
