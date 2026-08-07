using BlazorApp1.Models;
using BlazorApp1.Services.AuthService;
using BlazorApp1.Services.Caching;

namespace BlazorApp1.Services.App;

public class AuthSessionState(IAppAuthentication _auth, IAppService _appService, TokenVerificationCache _verification, LibraryCacheService _libraryCache)
{
    public bool      IsAuthenticated    { get; private set; }
    public bool      IsInitializing     { get; private set; } = true;
    public bool      IsResolvingSession { get; private set; }
    public UserInfo? CurrentUser        { get; private set; }
    public string?   PhotoUrl           { get; private set; }
    public string?   ProfileLoadError   { get; private set; }
    public bool      SignInModalOpen    { get; private set; }

    // Where to return once sign-in completes — set when a guest is interrupted mid-action
    // (e.g. pressing Buy Now) so the flow resumes rather than dumping them on the home page.
    public string? PendingBookUid { get; private set; }

    public bool IsBusy => IsInitializing || IsResolvingSession;

    public bool NeedsProfileSetup
        => !IsBusy && IsAuthenticated && CurrentUser is null && ProfileLoadError is null;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        _auth.StateChanged += HandleAuthStateChanged;

        try
        {
            await _auth.InitializeAsync();
            IsAuthenticated = await _auth.IsAuthenticatedAsync();

            if (IsAuthenticated)
                await LoadSessionAsync();
        }
        catch (Exception ex)
        {
            // A misconfigured or unreachable identity provider must not stop the app from
            // booting — the storefront works signed out, so degrade to guest instead.
            Console.WriteLine($"[Learn It-All] Auth initialization failed: {ex.Message}");
            IsAuthenticated = false;
        }

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
            await LoadSessionAsync();
        }
        else if (!IsAuthenticated && wasAuthenticated)
        {
            ResetLocalSession();
        }

        IsResolvingSession = false;
        OnChange?.Invoke();
    }

    // One verification per token, then the profile. Everything downstream reuses both.
    private async Task LoadSessionAsync()
    {
        try
        {
            ProfileLoadError = null;
            await _verification.EnsureVerifiedAsync();
            PhotoUrl    = await _auth.GetPhotoUrlAsync();
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

    public Task RefreshProfileAsync() => LoadSessionAsync();

    public void OpenSignInModal(string? pendingBookUid = null)
    {
        PendingBookUid  = pendingBookUid;
        SignInModalOpen = true;
        OnChange?.Invoke();
    }

    public void CloseSignInModal()
    {
        SignInModalOpen = false;
        OnChange?.Invoke();
    }

    // Parks a checkout that can't run yet — an authenticated user who still owes us a
    // profile. The store page picks it back up once setup completes.
    public void RememberPendingBook(string bookUid)
    {
        if (PendingBookUid == bookUid) return;

        PendingBookUid = bookUid;
        OnChange?.Invoke();
    }

    public string? TakePendingBookUid()
    {
        var uid = PendingBookUid;
        PendingBookUid = null;
        return uid;
    }

    public void UpdateLocalReadingProgress(string bookUid, int page, int totalPages)
    {
        var entry = CurrentUser?.Library.FirstOrDefault(l => l.Uid == bookUid);
        if (entry is null) return;

        entry.LastReadPage       = page;
        entry.LastReadTotalPages = totalPages;
        entry.LastReadAt         = DateTime.UtcNow;
        OnChange?.Invoke();
    }

    // A fresh purchase must show up in My Library right away, so its cache is dropped.
    public async Task OnPurchaseCompletedAsync()
    {
        _libraryCache.InvalidateMyLibrary();
        await RefreshProfileAsync();
    }

    public async Task SignOutAsync()
    {
        try   { await _appService.LogActivity("Logged out"); }
        catch { /* signing out must not fail on a logging call */ }

        ResetLocalSession();
        OnChange?.Invoke();

        await _auth.SignOutAsync();
    }

    private void ResetLocalSession()
    {
        IsAuthenticated = false;
        CurrentUser     = null;
        PhotoUrl        = null;
        SignInModalOpen = false;
        _verification.Clear();
        _libraryCache.ClearAll();
    }
}
