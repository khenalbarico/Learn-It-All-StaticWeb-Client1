namespace BlazorApp1.Models;

// A presigned R2 URL: pdf.js fetches the PDF directly from storage, so the file never
// passes through the Function App.
public class BookReadUrl
{
    public string   Url       { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
