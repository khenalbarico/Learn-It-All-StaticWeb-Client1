using BlazorApp1.Models;

namespace BlazorApp1.Services.AuthService;

public interface IAppAuthentication
{
    Task<AuthResult> SignInWithEmailAsync(string email, string password);
    Task RegisterWithEmailAsync(string email, string password);
    Task<AuthResult> CheckEmailVerifiedAsync();
    Task SendEmailVerificationAsync();
    Task SendPasswordResetEmailAsync(string email);
    Task<string> VerifyPasswordResetCodeAsync(string oobCode);
    Task ResetPasswordAsync(string oobCode, string newPassword);
    Task<bool> IsSamePasswordAsync(string email, string password);
    Task ChangePasswordAsync(string newPassword);
    Task<AuthResult> RefreshAsync();
    Task<bool> TryRestoreSessionAsync();
    bool IsAuthenticated { get; }
    void SignOut();
}
