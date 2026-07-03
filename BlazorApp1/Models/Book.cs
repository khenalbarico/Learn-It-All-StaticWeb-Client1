using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models;

public class Book
{
    [Required] public string Uid { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageCoverUrl { get; set; }
    public List<BookDocs> Documents { get; set; } = [];
}
