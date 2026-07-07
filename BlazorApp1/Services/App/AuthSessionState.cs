using BlazorApp1.Models;
using BlazorApp1.Services.AuthService;
using BlazorApp1.Services.Caching;

namespace BlazorApp1.Services.App;

public class AuthSessionState(IAppAuthentication _auth, IAppService _appService, LibraryCacheService _libraryCache)
{
    public bool IsAuthenticated { get; private set; }
    public bool IsInitializing { get; private set; } = true;
    public bool IsResolvingSession { get; private set; }
    public UserInfo? CurrentUser { get; private set; }
    public string? ProfileLoadError { get; private set; }
    public bool SignInModalOpen { get; private set; }

    // True while we're mid-transition (e.g. just came back from the identity
    // provider redirect) and haven't yet confirmed both the auth state AND
    // the profile. Consumers must treat this exactly like IsInitializing -
    // otherwise there's a window where a stale "guest" state can render with
    // clickable sign-in buttons a split second before the real state lands.
    public bool IsBusy => IsInitializing || IsResolvingSession;

    public bool NeedsProfileSetup
        => !IsBusy
        && IsAuthenticated
        && CurrentUser is null
        && ProfileLoadError is null;

    public event Action? OnChange;

    public async Task FastInitializeAsync()
    {
        _auth.StateChanged += HandleAuthStateChanged;

        IsAuthenticated = await _auth.IsAuthenticatedAsync();

        if (IsAuthenticated)
            await LoadProfileAsync();

        IsInitializing = false;
        OnChange?.Invoke();
    }

    private async void HandleAuthStateChanged()
    {
        IsResolvingSession = true;
        OnChange?.Invoke();

        var wasAuthenticated = IsAuthenticated;
        IsAuthenticated = await _auth.IsAuthenticatedAsync();

        if (IsAuthenticated && !wasAuthenticated)
        {
            SignInModalOpen = false;
            await LoadProfileAsync();
        }

        IsResolvingSession = false;
        OnChange?.Invoke();
    }

    public void OpenSignInModal()
    {
        SignInModalOpen = true;
        OnChange?.Invoke();
    }

    public void CloseSignInModal()
    {
        SignInModalOpen = false;
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

    public void BeginSignIn(string returnUrl = "auth")
        => _auth.SignIn(returnUrl);

    public async Task SignOutAsync()
    {
        await _appService.LogActivity("Logged out");

        IsAuthenticated = false;
        CurrentUser = null;
        SignInModalOpen = false;
        _libraryCache.ClearAll();
        OnChange?.Invoke();

        _auth.SignOut("/");
    }
}
