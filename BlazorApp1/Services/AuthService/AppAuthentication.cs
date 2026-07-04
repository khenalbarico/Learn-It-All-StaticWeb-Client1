using System.Text;
using System.Text.Json;
using Firebase.Auth;
using BlazorApp1.Models;

namespace BlazorApp1.Services.AuthService;

public class AppAuthentication(IFirebaseCfg _cfg, TokenCache _cache) : IAppAuthentication
{
    private readonly FirebaseAuthClient _client = _cfg.CreateAuthClient();
    private readonly HttpClient _httpClient = new();
    private UserCredential? _credential;

    public bool IsAuthenticated => _cache.TryGet(out _);

    public async Task<AuthResult> SignInWithEmailAsync(string email, string password)
    {
        try
        {
            _credential = await _client.SignInWithEmailAndPasswordAsync(email, password);
        }
        catch (FirebaseAuthException ex)
        {
            throw new Exception(FirebaseErrorTranslator.Translate(ex));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiUnavailableException("Could not reach Firebase. Please check your internet connection and try again.", ex);
        }

        if (!_credential.User.Info.IsEmailVerified)
            throw new EmailNotVerifiedException();

        return await BuildAndCacheAsync();
    }

    public async Task RegisterWithEmailAsync(string email, string password)
    {
        try
        {
            _credential = await _client.CreateUserWithEmailAndPasswordAsync(email, password);
        }
        catch (FirebaseAuthException ex)
        {
            throw new Exception(FirebaseErrorTranslator.Translate(ex));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiUnavailableException("Could not reach Firebase. Please check your internet connection and try again.", ex);
        }

        await SendVerificationEmailInternalAsync();
    }

    public async Task<AuthResult> CheckEmailVerifiedAsync()
    {
        if (_credential is null)
            throw new InvalidOperationException("No pending session to verify.");

        var idToken = await _credential.User.GetIdTokenAsync(forceRefresh: true);

        if (!ExtractEmailVerifiedClaim(idToken))
            throw new EmailNotVerifiedException();

        return await BuildAndCacheAsync();
    }

    public async Task SendEmailVerificationAsync()
    {
        if (_credential is null)
            throw new InvalidOperationException("No active user to send verification to.");

        await SendVerificationEmailInternalAsync();
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        await PostIdentityToolkitAsync("accounts:sendOobCode", new { requestType = "PASSWORD_RESET", email });
    }

    public async Task<string> VerifyPasswordResetCodeAsync(string oobCode)
    {
        var result = await PostIdentityToolkitAsync<ResetPasswordVerifyResponse>("accounts:resetPassword", new { oobCode });
        return result.Email;
    }

    public async Task ResetPasswordAsync(string oobCode, string newPassword)
    {
        await PostIdentityToolkitAsync("accounts:resetPassword", new { oobCode, newPassword });
    }

    public async Task<bool> IsSamePasswordAsync(string email, string password)
    {
        var probeClient = _cfg.CreateAuthClient();

        try
        {
            await probeClient.SignInWithEmailAndPasswordAsync(email, password);
            return true;
        }
        catch (FirebaseAuthException ex) when (IsAccountBlockingError(ex))
        {
            throw new Exception(FirebaseErrorTranslator.Translate(ex));
        }
        catch (FirebaseAuthException)
        {
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiUnavailableException("Could not reach Firebase. Please check your internet connection and try again.", ex);
        }
    }

    private static bool IsAccountBlockingError(FirebaseAuthException ex)
    {
        var msg = ex.Message?.ToUpperInvariant() ?? "";
        return msg.Contains("TOO_MANY_ATTEMPTS") || msg.Contains("TOO_MANY_REQUESTS") || msg.Contains("USER_DISABLED");
    }

    private record ResetPasswordVerifyResponse(string Email);

    public async Task ChangePasswordAsync(string newPassword)
    {
        var user = _credential?.User ?? _client.User;
        if (user is null)
            throw new InvalidOperationException("No active session to change the password for.");

        var idToken = await user.GetIdTokenAsync(forceRefresh: false);
        await PostIdentityToolkitAsync("accounts:update", new { idToken, password = newPassword, returnSecureToken = false });
    }

    public async Task<AuthResult> RefreshAsync()
    {
        var user = _credential?.User ?? _client.User;
        if (user is null)
            throw new InvalidOperationException("No active session to refresh.");

        string token;
        try
        {
            token = await user.GetIdTokenAsync(forceRefresh: true);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiUnavailableException("Could not refresh your session. Please check your internet connection and try again.", ex);
        }

        var auth = new AuthResult
        {
            Token = token,
            Uid = user.Uid,
            Email = user.Info.Email ?? ""
        };

        await _cache.SetAsync(auth);
        return auth;
    }

    public Task<bool> TryRestoreSessionAsync()
        => _cache.TryRestoreAsync();

    public void SignOut()
    {
        try
        {
            _client.SignOut();
        }
        catch (Exception)
        {
        }
        finally
        {
            _credential = null;
            _cache.Clear();
        }
    }

    private async Task SendVerificationEmailInternalAsync()
    {
        var idToken = await _credential!.User.GetIdTokenAsync(forceRefresh: false);
        await PostIdentityToolkitAsync("accounts:sendOobCode", new { requestType = "VERIFY_EMAIL", idToken });
    }

    private async Task PostIdentityToolkitAsync(string endpoint, object body)
        => await SendIdentityToolkitRequestAsync(endpoint, body);

    private async Task<T> PostIdentityToolkitAsync<T>(string endpoint, object body)
    {
        var content = await SendIdentityToolkitRequestAsync(endpoint, body);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private async Task<string> SendIdentityToolkitRequestAsync(string endpoint, object body)
    {
        HttpResponseMessage response;

        try
        {
            var json = JsonSerializer.Serialize(body);
            response = await _httpClient.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/{endpoint}?key={_cfg.ApiKey}",
                new StringContent(json, Encoding.UTF8, "application/json"));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiUnavailableException("Could not reach Firebase. Please check your internet connection and try again.", ex);
        }

        using (response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(FirebaseErrorTranslator.Translate(ExtractFirebaseErrorCode(content)));

            return content;
        }
    }

    private static string ExtractFirebaseErrorCode(string errorBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            return doc.RootElement.GetProperty("error").GetProperty("message").GetString() ?? "";
        }
        catch (Exception)
        {
            return "";
        }
    }

    private static bool ExtractEmailVerifiedClaim(string idToken)
    {
        var parts = idToken.Split('.');
        if (parts.Length != 3) return false;

        var base64 = parts[1].Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("email_verified", out var prop) && prop.GetBoolean();
    }

    private async Task<AuthResult> BuildAndCacheAsync()
    {
        var auth = new AuthResult
        {
            Token = await _credential!.User.GetIdTokenAsync(forceRefresh: false),
            Uid = _credential.User.Uid,
            Email = _credential.User.Info.Email ?? ""
        };

        await _cache.SetAsync(auth);
        return auth;
    }
}
