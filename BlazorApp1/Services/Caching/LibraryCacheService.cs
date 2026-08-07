using BlazorApp1.Models;

namespace BlazorApp1.Services.Caching;

public class LibraryCacheService
{
    private List<Book>? _allBooksCache;
    private List<Book>? _myLibraryCache;

    // Presigned read URLs are reusable until they expire, so re-opening a book within
    // a session doesn't cost another API call.
    private readonly Dictionary<string, BookReadUrl> _readUrlCache = [];

    public bool TryGetAllBooks(out List<Book> books)
    {
        books = _allBooksCache ?? [];
        return _allBooksCache is not null;
    }

    public void SetAllBooks(List<Book> books) => _allBooksCache = books;

    public bool TryGetMyLibrary(out List<Book> books)
    {
        books = _myLibraryCache ?? [];
        return _myLibraryCache is not null;
    }

    public void SetMyLibrary(List<Book> books) => _myLibraryCache = books;

    public void InvalidateMyLibrary() => _myLibraryCache = null;

    public bool TryGetReadUrl(string bookUid, out string url)
    {
        url = "";

        if (!_readUrlCache.TryGetValue(bookUid, out var entry)) return false;
        if (DateTime.UtcNow >= entry.ExpiresAt.AddMinutes(-5))
        {
            _readUrlCache.Remove(bookUid);
            return false;
        }

        url = entry.Url;
        return true;
    }

    public void SetReadUrl(string bookUid, BookReadUrl readUrl) => _readUrlCache[bookUid] = readUrl;

    public void ClearAll()
    {
        _allBooksCache  = null;
        _myLibraryCache = null;
        _readUrlCache.Clear();
    }
}
