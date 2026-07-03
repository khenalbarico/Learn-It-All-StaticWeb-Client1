using BlazorApp1.Models;

namespace BlazorApp1.Services.ApiService;

public class ApiRouting
{
    public static readonly Dictionary<ApiFunctions, string> Routes = new()
    {
        [ApiFunctions.TryGetUser]         = "TryGetUser",
        [ApiFunctions.GetAllBooks]        = "GetAllBooks",
        [ApiFunctions.GetBooksByCategory] = "GetBooksByCategory",
        [ApiFunctions.CreateUser]         = "CreateUser",
        [ApiFunctions.RecordPurchase]     = "RecordPurchase",
        [ApiFunctions.GetBookUrl]         = "GetBookUrl",
        [ApiFunctions.GetMyLibraryBooks]  = "GetMyLibraryBooks",
    };
}
