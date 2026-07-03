using System.Text.Json;
using BlazorApp1.Models;
using BlazorApp1.Services.App;

namespace BlazorApp1.Services.AuthService;

public class BrowserAuthPersistence(JsInteropService _jsInterop) : IAuthPersistence
{
    private const string StorageKey = "auth_session";

    private record StoredSession(string Token, string Uid, string Email, long ExpiresAtTicks);

    public async Task SaveAsync(AuthResult auth, DateTime expiresAt)
    {
        var stored = new StoredSession(auth.Token, auth.Uid, auth.Email, expiresAt.Ticks);
        await _jsInterop.SetItemAsync(StorageKey, JsonSerializer.Serialize(stored));
    }

    public async Task<(AuthResult? Auth, DateTime ExpiresAt)> TryLoadAsync()
    {
        var raw = await _jsInterop.GetItemAsync(StorageKey);
        if (string.IsNullOrEmpty(raw))
            return (null, DateTime.MinValue);

        var stored = JsonSerializer.Deserialize<StoredSession>(raw);
        if (stored is null || string.IsNullOrEmpty(stored.Uid))
            return (null, DateTime.MinValue);

        var auth = new AuthResult { Token = stored.Token, Uid = stored.Uid, Email = stored.Email };
        var expiresAt = new DateTime(stored.ExpiresAtTicks, DateTimeKind.Utc);
        return (auth, expiresAt);
    }

    public void Clear()
    {
        _ = _jsInterop.RemoveItemAsync(StorageKey);
    }
}
