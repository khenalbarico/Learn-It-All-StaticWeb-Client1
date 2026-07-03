using BlazorApp1.Models;

namespace BlazorApp1.Services.Caching;

public class LibraryCacheService
{
    private readonly Dictionary<string, List<Book>> _categoryCache = new(StringComparer.OrdinalIgnoreCase);
    private List<Book>? _myLibraryCache;

    public bool TryGetCategory(string category, out List<Book> books)
        => _categoryCache.TryGetValue(category, out books!);

    public void SetCategory(string category, List<Book> books)
        => _categoryCache[category] = books;

    public bool TryGetMyLibrary(out List<Book> books)
    {
        books = _myLibraryCache ?? [];
        return _myLibraryCache is not null;
    }

    public void SetMyLibrary(List<Book> books)
        => _myLibraryCache = books;

    public void InvalidateMyLibrary()
        => _myLibraryCache = null;

    public void ClearAll()
    {
        _categoryCache.Clear();
        _myLibraryCache = null;
    }
}
