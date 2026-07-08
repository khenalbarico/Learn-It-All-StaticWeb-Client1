using BlazorApp1.Models;

namespace BlazorApp1.Services.App;

public interface IAppService
{
    Task<List<Book>> GetAllBooks();
    Task<List<Book>> GetBooksByCategory(string category);
    Task<UserInfo?> TryGetUserInfo();
    Task CreateUser(string firstName, string lastName, string phoneNumber);
    Task RecordPurchase(string bookUid, string orderId, string purchaseToken, decimal priceAtPurchase);
    Task<byte[]> GetBookDocumentBytes(string bookUid, string docUid);
    Task<List<Book>> GetMyLibraryBooks();
    Task LogActivity(string activity);
    Task UpdateUserKeywords(List<string> keywords);
    Task SaveReadingProgress(string bookUid, string docUid, int page, int totalPages);
    Task SetFavorite(string bookUid, bool isFavorite);
    Task AddToCart(string bookUid, bool isPremium = false);
    Task RemoveFromCart(string bookUid);
}
