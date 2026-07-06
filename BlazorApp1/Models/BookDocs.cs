namespace BlazorApp1.Models;

public class BookDocs
{
    public string Uid { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Folder { get; set; } = string.Empty;
    public bool IsPremium { get; set; }
}
