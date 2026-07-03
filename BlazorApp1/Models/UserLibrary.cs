namespace BlazorApp1.Models;

public class UserLibrary
{
    public string? Uid { get; set; }
    public string? OrderId { get; set; }
    public decimal PriceAtPurchased { get; set; }
    public DateTime PurchasedAt { get; set; }
}
