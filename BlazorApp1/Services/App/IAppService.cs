using BlazorApp1.Models;

namespace BlazorApp1.Services.App;

public interface IAppService
{
    Task<List<Book>> GetAllBooks();
    Task<List<Book>> GetMyLibraryBooks();
    Task<UserInfo?> TryGetUserInfo();
    Task CreateUser(string firstName, string lastName, string phoneNumber);
    Task<BookReadUrl> GetBookReadUrl(string bookUid);
    Task LogActivity(string activity);
    Task SaveReadingProgress(string bookUid, int page, int totalPages);
    Task SetFavorite(string bookUid, bool isFavorite);
}
