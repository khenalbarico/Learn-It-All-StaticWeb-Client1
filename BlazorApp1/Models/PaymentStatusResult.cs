namespace BlazorApp1.Models;

public class PaymentStatusResult
{
    public string Status { get; set; } = string.Empty;
    public string? QrImageUrl { get; set; }
    public DateTime ExpiresAt { get; set; }
}
