using BlazorApp1.Models;

namespace BlazorApp1.Services.App;

public interface IPaymentService
{
    Task<CreatePaymentResult> CreatePaymentIntent(List<string> bookUids);
    Task<PaymentStatusResult> GetPaymentStatus(string paymentIntentId, bool forceVerify = false);
}
