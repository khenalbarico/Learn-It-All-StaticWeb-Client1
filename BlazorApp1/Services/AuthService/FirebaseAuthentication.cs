using BlazorApp1.Models;
using Microsoft.JSInterop;

namespace BlazorApp1.Services.AuthService;

public class FirebaseAuthentication(IJSRuntime _js, FirebaseCfg _cfg) : IAppAuthentication, IAsyncDisposable
{
    // Refresh slightly early so a token can't lapse mid-flight between here and the API.
    private static readonly TimeSpan ExpiryLeeway = TimeSpan.FromMinutes(1);

    private IJSObjectReference?             _module;
    private DotNetObjectReference<FirebaseAuthentication>? _selfRef;
    private AuthResult?                     _cached;

    public event Action? StateChanged;

    public async Task InitializeAsync()
    {
        if (_module is not null) return;

        _module  = await _js.InvokeAsync<IJSObjectReference>("import", "./js/firebaseAuth.js");
        _selfRef = DotNetObjectReference.Create(this);

        await _module.InvokeVoidAsync("initialize", _cfg, _selfRef);
    }

    [JSInvokable]
    public void OnAuthStateChanged(bool isAuthenticated)
    {
        _cached = null;
        StateChanged?.Invoke();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<bool>("isAuthenticated");
    }

    public async Task<AuthResult> GetAuthAsync()
        => await TryGetAuthAsync()
           ?? throw new UnauthorizedAccessException("Your session has expired. Please sign in again.");

    public async Task<AuthResult?> TryGetAuthAsync()
    {
        await InitializeAsync();

        if (_cached is not null && DateTime.UtcNow < _cached.ExpiresAt - ExpiryLeeway)
            return _cached;

        var dto = await _module!.InvokeAsync<FirebaseUserDto?>("getAuth_", _cached is not null);
        if (dto is null)
        {
            _cached = null;
            return null;
        }

        _cached = new AuthResult
        {
            Token       = dto.Token,
            Uid         = dto.Uid,
            Email       = dto.Email,
            DisplayName = dto.DisplayName,
            PhotoUrl    = dto.PhotoUrl,
            ExpiresAt   = DateTime.TryParse(dto.ExpiresAt, out var parsed)
                              ? parsed.ToUniversalTime()
                              : DateTime.UtcNow.AddMinutes(55)
        };

        return _cached;
    }

    public async Task<ProfileHint> GetProfileHintAsync()
    {
        var auth = await TryGetAuthAsync();
        if (string.IsNullOrWhiteSpace(auth?.DisplayName))
            return new ProfileHint(null, null, null);

        var parts = auth.DisplayName.Trim().Split(' ', 2);
        return new ProfileHint(parts[0], parts.Length > 1 ? parts[1] : null, null);
    }

    public async Task<string?> GetPhotoUrlAsync()
        => (await TryGetAuthAsync())?.PhotoUrl;

    public async Task<string?> SignInWithProviderAsync(string providerId)
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<string?>("signInWithProvider", providerId);
    }

    public async Task<string?> SignInWithEmailAsync(string email, string password)
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<string?>("signInWithEmail", email, password);
    }

    public async Task<string?> RegisterWithEmailAsync(string email, string password, string? displayName)
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<string?>("registerWithEmail", email, password, displayName);
    }

    public async Task<string?> SendPasswordResetAsync(string email)
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<string?>("sendPasswordReset", email);
    }

    public async Task SignOutAsync()
    {
        await InitializeAsync();
        _cached = null;
        await _module!.InvokeVoidAsync("signOutUser");
    }

    public async ValueTask DisposeAsync()
    {
        _selfRef?.Dispose();

        if (_module is not null)
        {
            try   { await _module.DisposeAsync(); }
            catch (JSDisconnectedException) { }
        }
    }

    private sealed class FirebaseUserDto
    {
        public string  Token       { get; set; } = "";
        public string  Uid         { get; set; } = "";
        public string? Email       { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl    { get; set; }
        public string? ExpiresAt   { get; set; }
    }
}
