using BlazorApp1.Models;

namespace BlazorApp1.Services.AuthService;

public interface IAppAuthentication
{
    event Action? StateChanged;

    Task InitializeAsync();
    Task<bool> IsAuthenticatedAsync();

    // Returns the current ID token, refreshing it only when the cached one has expired.
    Task<AuthResult> GetAuthAsync();
    Task<AuthResult?> TryGetAuthAsync();
    Task<ProfileHint> GetProfileHintAsync();
    Task<string?> GetPhotoUrlAsync();

    Task<string?> SignInWithProviderAsync(string providerId);
    Task<string?> SignInWithEmailAsync(string email, string password);
    Task<string?> RegisterWithEmailAsync(string email, string password, string? displayName);
    Task<string?> SendPasswordResetAsync(string email);
    Task SignOutAsync();
}
