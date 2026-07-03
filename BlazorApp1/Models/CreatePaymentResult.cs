namespace BlazorApp1.Models;

public class CreatePaymentResult
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string? QrImageUrl { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
