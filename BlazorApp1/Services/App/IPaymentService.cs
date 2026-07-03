using BlazorApp1.Models;

namespace BlazorApp1.Services.App;

public interface IPaymentService
{
    Task<CreatePaymentResult> CreatePaymentIntent(string bookUid);
    Task<PaymentStatusResult> GetPaymentStatus(string paymentIntentId);
}
