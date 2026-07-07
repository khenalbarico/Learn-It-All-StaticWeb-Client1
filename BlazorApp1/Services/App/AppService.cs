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

    public async Task LogActivity(string activity)
    {
        try
        {
            await _api.SubmitAsync(ApiFunctions.LogActivity, new { Activity = activity });
        }
        catch (Exception)
        {
            // best-effort; a failed activity log must never affect the user's actual action
        }
    }

    public Task UpdateUserKeywords(List<string> keywords)
        => _api.SubmitAsync(ApiFunctions.UpdateUserKeywords, new { Keywords = keywords });

    public async Task SaveReadingProgress(string bookUid, string docUid, int page, int totalPages)
    {
        try
        {
            await _api.SubmitAsync(ApiFunctions.SaveReadingProgress, new { BookUid = bookUid, DocUid = docUid, Page = page, TotalPages = totalPages });
        }
        catch (Exception)
        {
            // best-effort; losing a progress-save must never interrupt reading
        }
    }

    public Task SetFavorite(string bookUid, bool isFavorite)
        => _api.SubmitAsync(ApiFunctions.SetFavorite, new { BookUid = bookUid, IsFavorite = isFavorite });

    public async Task AddToCart(string bookUid, bool isPremium = false)
    {
        try
        {
            await _api.SubmitAsync(ApiFunctions.AddToCart, new { BookUid = bookUid, IsPremium = isPremium });
        }
        catch (Exception)
        {
            // best-effort; the optimistic local cart update already reflected the action
        }
    }

    public async Task RemoveFromCart(string bookUid)
    {
        try
        {
            await _api.SubmitAsync(ApiFunctions.RemoveFromCart, new { BookUid = bookUid });
        }
        catch (Exception)
        {
            // best-effort; the optimistic local cart update already reflected the action
        }
    }
}
