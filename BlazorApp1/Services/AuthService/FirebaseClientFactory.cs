using Firebase.Auth;
using Firebase.Auth.Providers;

namespace BlazorApp1.Services.AuthService;

public static class FirebaseClientFactory
{
    public static FirebaseAuthClient CreateAuthClient(this IFirebaseCfg cfg)
    {
        var authConfig = new FirebaseAuthConfig
        {
            ApiKey = cfg.ApiKey,
            AuthDomain = cfg.AuthDomain,
            Providers = [new EmailProvider()]
        };

        return new FirebaseAuthClient(authConfig);
    }
}
