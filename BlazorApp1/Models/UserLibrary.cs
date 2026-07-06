namespace BlazorApp1.Models;

public class UserLibrary
{
    public string?                Uid                { get; set; }
    public string?                OrderId            { get; set; }
    public decimal                PriceAtPurchased   { get; set; }
    public DateTime               PurchasedAt        { get; set; }
    public bool                   IsPremium          { get; set; }
    public bool                   IsFavorite         { get; set; }
    public List<DocumentProgress> DocumentsProgress  { get; set; } = [];
    public string?                LastReadDocUid     { get; set; }
    public int                    LastReadPage       { get; set; }
    public int                    LastReadTotalPages { get; set; }
    public DateTime?              LastReadAt         { get; set; }
}
