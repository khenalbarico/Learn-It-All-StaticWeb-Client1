using BlazorApp1.Models;

namespace BlazorApp1.Services.Caching;

public class LibraryCacheService
{
    private const int MaxCachedDocuments = 5;

    private readonly Dictionary<string, List<Book>> _categoryCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, byte[]> _documentCache = [];
    private readonly List<string> _documentCacheOrder = [];
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

    public bool TryGetDocument(string bookUid, string docUid, out byte[] bytes)
        => _documentCache.TryGetValue(DocumentKey(bookUid, docUid), out bytes!);

    public void SetDocument(string bookUid, string docUid, byte[] bytes)
    {
        var key = DocumentKey(bookUid, docUid);

        if (!_documentCache.ContainsKey(key) && _documentCacheOrder.Count >= MaxCachedDocuments)
        {
            var oldest = _documentCacheOrder[0];
            _documentCacheOrder.RemoveAt(0);
            _documentCache.Remove(oldest);
        }

        _documentCache[key] = bytes;
        _documentCacheOrder.Remove(key);
        _documentCacheOrder.Add(key);
    }

    private static string DocumentKey(string bookUid, string docUid) => $"{bookUid}/{docUid}";

    public void ClearAll()
    {
        _categoryCache.Clear();
        _myLibraryCache = null;
        _documentCache.Clear();
        _documentCacheOrder.Clear();
    }
}
