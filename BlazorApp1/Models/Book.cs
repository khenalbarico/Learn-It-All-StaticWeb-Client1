namespace BlazorApp1.Models;

public class Book
{
    public string  Uid           { get; set; } = string.Empty;
    public string? Title         { get; set; }
    public string? Description   { get; set; }
    public double  Price         { get; set; }
    public string? ImageCoverUrl { get; set; }
}
