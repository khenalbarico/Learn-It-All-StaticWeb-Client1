using BlazorApp1.Models;
using BlazorApp1.Services.ApiService;

namespace BlazorApp1.Services.AuthService;

// The API verifies a Firebase ID token with Firebase Admin, which costs a Function
// invocation. Once a token is verified we hold that result until the token itself
// expires, so a whole reading session costs one verification instead of one per action.
public class TokenVerificationCache(IApiClient _api, IAppAuthentication _auth)
{
    private VerifiedAuth? _verified;
    private string?       _verifiedToken;

    public VerifiedAuth? Current => IsStillValid(_verified) ? _verified : null;

    public async Task<VerifiedAuth?> EnsureVerifiedAsync()
    {
        var auth = await _auth.TryGetAuthAsync();
        if (auth is null)
        {
            Clear();
            return null;
        }

        // Same token and still in date? Nothing to ask the API.
        if (_verifiedToken == auth.Token && IsStillValid(_verified))
            return _verified;

        _verified      = await _api.GetAsync<VerifiedAuth>(ApiFunctions.VerifyAuth);
        _verifiedToken = auth.Token;

        return _verified;
    }

    public void Clear()
    {
        _verified      = null;
        _verifiedToken = null;
    }

    private static bool IsStillValid(VerifiedAuth? verified)
        => verified is not null && DateTime.UtcNow < verified.ExpiresAt.AddMinutes(-1);
}
