using BlazorApp1.Models;
using BlazorApp1.Services.ApiService;

namespace BlazorApp1.Services.App;

public class PaymentService(IApiClient _api) : IPaymentService
{
    public Task<CreatePaymentResult> CreatePaymentIntent(string bookUid)
        => _api.SubmitAsync<CreatePaymentResult>(ApiFunctions.CreatePaymentIntent, new { BookUid = bookUid });

    public Task<PaymentStatusResult> GetPaymentStatus(string paymentIntentId, bool forceVerify = false)
        => _api.GetAsync<PaymentStatusResult>(ApiFunctions.GetPaymentStatus, new { PaymentIntentId = paymentIntentId, ForceVerify = forceVerify });
}
