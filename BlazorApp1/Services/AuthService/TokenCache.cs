using BlazorApp1.Models;

namespace BlazorApp1.Services.AuthService;

public class TokenCache(IAuthPersistence _persistence)
{
    private AuthResult? _cached;
    private DateTime _expiresAt = DateTime.MinValue;

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan SafetyBuffer = TimeSpan.FromMinutes(5);

    public bool TryGet(out AuthResult? result)
    {
        if (_cached is not null && DateTime.UtcNow < _expiresAt)
        {
            result = _cached;
            return true;
        }

        result = null;
        return false;
    }

    public async Task SetAsync(AuthResult auth)
    {
        _expiresAt = DateTime.UtcNow.Add(TokenLifetime - SafetyBuffer);
        _cached = auth;
        await _persistence.SaveAsync(auth, _expiresAt);
    }

    public async Task<bool> TryRestoreAsync()
    {
        var (auth, expiresAt) = await _persistence.TryLoadAsync();

        if (auth is null || DateTime.UtcNow >= expiresAt)
        {
            _persistence.Clear();
            return false;
        }

        _cached = auth;
        _expiresAt = expiresAt;
        return true;
    }

    public void Clear()
    {
        _cached = null;
        _expiresAt = DateTime.MinValue;
        _persistence.Clear();
    }
}
