using BlazorApp1.Models;

namespace BlazorApp1.Services.AuthService;

public interface IAppAuthentication
{
    Task<bool> IsAuthenticatedAsync();
    Task<AuthResult> GetAuthAsync();
    void SignIn(string returnUrl);
    void SignOut(string returnUrl);
}
