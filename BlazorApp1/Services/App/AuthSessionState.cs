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

        if (IsAuthenticated)
            await LoadProfileAsync();

        IsInitializing = false;
        OnChange?.Invoke();
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

    public void UpdateLocalReadingProgress(string bookUid, string docUid, int page, int totalPages)
    {
        var entry = CurrentUser?.Library.FirstOrDefault(l => l.Uid == bookUid);
        if (entry is null) return;

        var now = DateTime.UtcNow;

        var docProgress = entry.DocumentsProgress.FirstOrDefault(d => d.DocUid == docUid);
        if (docProgress is null)
        {
            docProgress = new DocumentProgress { DocUid = docUid };
            entry.DocumentsProgress.Add(docProgress);
        }

        docProgress.Page = page;
        docProgress.TotalPages = totalPages;
        docProgress.LastReadAt = now;

        entry.LastReadDocUid = docUid;
        entry.LastReadPage = page;
        entry.LastReadTotalPages = totalPages;
        entry.LastReadAt = now;
        OnChange?.Invoke();
    }

    public void AddToCartLocal(string bookUid)
    {
        if (CurrentUser is null) return;
        if (CurrentUser.Cart.Any(c => c.BookUid == bookUid)) return;

        CurrentUser.Cart.Add(new CartItem { BookUid = bookUid, AddedAt = DateTime.UtcNow });
        OnChange?.Invoke();
    }

    public void RemoveFromCartLocal(string bookUid)
    {
        if (CurrentUser is null) return;
        if (CurrentUser.Cart.RemoveAll(c => c.BookUid == bookUid) > 0)
            OnChange?.Invoke();
    }

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
