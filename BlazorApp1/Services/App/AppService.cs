using System.Net;
using BlazorApp1.Models;
using BlazorApp1.Services.ApiService;

namespace BlazorApp1.Services.App;

public class AppService(IApiClient _api) : IAppService
{
    public Task<List<Book>> GetAllBooks()
        => _api.GetAsync<List<Book>>(ApiFunctions.GetAllBooks);

    public Task<List<Book>> GetBooksByCategory(string category)
        => _api.GetAsync<List<Book>>(ApiFunctions.GetBooksByCategory, new { Category = category });

    public async Task<UserInfo?> TryGetUserInfo()
    {
        try
        {
            return await _api.GetAsync<UserInfo>(ApiFunctions.TryGetUser);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task CreateUser(string firstName, string lastName, string phoneNumber)
        => _api.SubmitAsync(ApiFunctions.CreateUser, new
        {
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber
        });

    public Task RecordPurchase(string bookUid, string orderId, string purchaseToken, decimal priceAtPurchase)
        => _api.SubmitAsync(ApiFunctions.RecordPurchase, new
        {
            BookUid = bookUid,
            OrderId = orderId,
            PurchaseToken = purchaseToken,
            PriceAtPurchase = priceAtPurchase
        });

    public Task<byte[]> GetBookDocumentBytes(string bookUid, string docUid)
        => _api.GetBytesAsync(ApiFunctions.StreamBookDocument, new { BookUid = bookUid, DocUid = docUid });

    public Task<List<Book>> GetMyLibraryBooks()
        => _api.GetAsync<List<Book>>(ApiFunctions.GetMyLibraryBooks);
}
