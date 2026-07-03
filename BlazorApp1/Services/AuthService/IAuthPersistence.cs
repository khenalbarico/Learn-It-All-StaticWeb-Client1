using BlazorApp1.Models;

namespace BlazorApp1.Services.AuthService;

public interface IAuthPersistence
{
    Task SaveAsync(AuthResult auth, DateTime expiresAt);
    Task<(AuthResult? Auth, DateTime ExpiresAt)> TryLoadAsync();
    void Clear();
}
