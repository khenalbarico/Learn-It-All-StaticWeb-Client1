namespace BlazorApp1.Models;

public class CartItem
{
    public string BookUid { get; set; } = "";
    public DateTime AddedAt { get; set; }
    public bool IsPremium { get; set; }
}
