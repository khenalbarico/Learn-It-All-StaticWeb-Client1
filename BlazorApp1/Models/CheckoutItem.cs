namespace BlazorApp1.Models;

public record CheckoutItem(string BookUid, string Title, string? CoverUrl, decimal Price, string? Description = null, decimal? PremiumPrice = null, bool IsPremium = false, bool IsUpgrade = false)
{
    public decimal EffectivePrice => IsPremium && PremiumPrice is decimal premium
        ? IsUpgrade ? premium - Price : premium
        : Price;
}
