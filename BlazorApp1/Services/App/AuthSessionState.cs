using BlazorApp1.Models;
using BlazorApp1.Services.AuthService;
using BlazorApp1.Services.Caching;

namespace BlazorApp1.Services.App;

public class AuthSessionState(IAppAuthentication _auth, IAppService _appService, LibraryCacheService _libraryCache)
{
    public bool IsAuthenticated { get; private set; }
    public bool IsInitializing { get; private set; } = true;
    public UserInfo? CurrentUser { get; private set; }
    public string? ProfileLoadError { get; private set; }

    public event Action? OnChange;

    public async Task FastInitializeAsync()
    {
        await _auth.TryRestoreSessionAsync();
        IsAuthenticated = _auth.IsAuthenticated;
        IsInitializing = false;
        OnChange?.Invoke();

        if (IsAuthenticated)
            _ = LoadProfileAsync();
    }

    public async Task LoadProfileAsync()
    {
        try
        {
            ProfileLoadError = null;
            CurrentUser = await _appService.TryGetUserInfo();
        }
        catch (ApiUnavailableException ex)
        {
            ProfileLoadError = ex.Message;
        }
        catch (Exception)
        {
            ProfileLoadError = "Could not load your profile. Please try again.";
        }
        finally
        {
            OnChange?.Invoke();
        }
    }

    public Task RefreshProfileAsync() => LoadProfileAsync();

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        var result = await _auth.SignInWithEmailAsync(email, password);
        IsAuthenticated = true;
        OnChange?.Invoke();
        _ = LoadProfileAsync();
        _ = _appService.LogActivity("Logged in");
        return result;
    }

    public Task RegisterAsync(string email, string password)
        => _auth.RegisterWithEmailAsync(email, password);

    public async Task CompleteVerificationAsync()
    {
        await _auth.CheckEmailVerifiedAsync();
        IsAuthenticated = true;
        OnChange?.Invoke();
        _ = LoadProfileAsync();
    }

    public async Task SignOutAsync()
    {
        await _appService.LogActivity("Logged out");

        _auth.SignOut();
        IsAuthenticated = false;
        CurrentUser = null;
        _libraryCache.ClearAll();
        OnChange?.Invoke();
    }
}
