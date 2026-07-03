using BlazorApp1.Models;

namespace BlazorApp1.Services.AuthService;

public interface IAppAuthentication
{
    Task<AuthResult> SignInWithEmailAsync(string email, string password);
    Task RegisterWithEmailAsync(string email, string password);
    Task<AuthResult> CheckEmailVerifiedAsync();
    Task SendEmailVerificationAsync();
    Task ChangePasswordAsync(string newPassword);
    Task<AuthResult> RefreshAsync();
    Task<bool> TryRestoreSessionAsync();
    bool IsAuthenticated { get; }
    void SignOut();
}
