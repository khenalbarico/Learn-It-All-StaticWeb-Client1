using System.Net;
using BlazorApp1.Models;
using BlazorApp1.Services.ApiService;
using BlazorApp1.Services.Caching;

namespace BlazorApp1.Services.App;

public class AppService(IApiClient _api, LibraryCacheService _cache) : IAppService
{
    // Book metadata is fetched once per session and served from cache thereafter —
    // browsing the store must not cost a Function call per navigation.
    public async Task<List<Book>> GetAllBooks()
    {
        if (_cache.TryGetAllBooks(out var cached)) return cached;

        var books = await _api.GetAsync<List<Book>>(ApiFunctions.GetAllBooks);
        _cache.SetAllBooks(books);
        return books;
    }

    // Same cache pattern; invalidated on purchase so a new book shows up immediately.
    public async Task<List<Book>> GetMyLibraryBooks()
    {
        if (_cache.TryGetMyLibrary(out var cached)) return cached;

        var books = await _api.GetAsync<List<Book>>(ApiFunctions.GetMyLibraryBooks);
        _cache.SetMyLibrary(books);
        return books;
    }

    public async Task<UserInfo?> TryGetUserInfo()
    {
        try
        {
            return await _api.GetAsync<UserInfo>(ApiFunctions.TryGetUser);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // No profile record yet — the caller shows the "tell me about yourself" form.
            return null;
        }
    }

    public Task CreateUser(string firstName, string lastName, string phoneNumber)
        => _api.SubmitAsync(ApiFunctions.CreateUser, new { FirstName = firstName, LastName = lastName, PhoneNumber = phoneNumber });

    public Task<BookReadUrl> GetBookReadUrl(string bookUid)
        => _api.GetAsync<BookReadUrl>(ApiFunctions.GetBookReadUrl, new { BookUid = bookUid });

    public Task LogActivity(string activity)
        => _api.SubmitAsync(ApiFunctions.LogActivity, new { Activity = activity });

    public Task SaveReadingProgress(string bookUid, int page, int totalPages)
        => _api.SubmitAsync(ApiFunctions.SaveReadingProgress, new { BookUid = bookUid, Page = page, TotalPages = totalPages });

    public Task SetFavorite(string bookUid, bool isFavorite)
        => _api.SubmitAsync(ApiFunctions.SetFavorite, new { BookUid = bookUid, IsFavorite = isFavorite });
}
