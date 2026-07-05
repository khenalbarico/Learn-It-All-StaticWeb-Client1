namespace BlazorApp1.Models;

public record CheckoutItem(string BookUid, string Title, string? CoverUrl, decimal Price, string? Description = null);
