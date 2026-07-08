using BlazorApp1.Models;

namespace BlazorApp1.Services.AuthService;

public interface IAppAuthentication
{
    event Action? StateChanged;
    Task<bool> IsAuthenticatedAsync();
    Task<AuthResult> GetAuthAsync();
    Task<ProfileHint> GetProfileHintAsync();
    void SignIn(string returnUrl);
    void SignOut(string returnUrl);
}
