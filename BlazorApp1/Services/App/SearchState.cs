namespace BlazorApp1.Services.App;

// The header search box and the catalogue page live in different components, so the
// query is held here rather than passed through the layout.
public static class SearchState
{
    public static string Query { get; private set; } = "";

    public static event Action? OnChange;

    public static void SetQuery(string query)
    {
        if (Query == query) return;

        Query = query;
        OnChange?.Invoke();
    }

    public static bool Matches(Models.Book book)
        => string.IsNullOrWhiteSpace(Query)
        || (book.Title?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false)
        || (book.Description?.Contains(Query, StringComparison.OrdinalIgnoreCase) ?? false);
}
