using BlazorApp1.Models;

namespace BlazorApp1.Services.ApiService;

public static class ApiRouting
{
    public static readonly Dictionary<ApiFunctions, string> Routes = new()
    {
        [ApiFunctions.VerifyAuth]          = "VerifyAuth",
        [ApiFunctions.TryGetUser]          = "TryGetUser",
        [ApiFunctions.CreateUser]          = "CreateUser",
        [ApiFunctions.GetAllBooks]         = "GetAllBooks",
        [ApiFunctions.GetMyLibraryBooks]   = "GetMyLibraryBooks",
        [ApiFunctions.GetBookReadUrl]      = "GetBookReadUrl",
        [ApiFunctions.CreatePaymentIntent] = "CreatePaymentIntent",
        [ApiFunctions.GetPaymentStatus]    = "GetPaymentStatus",
        [ApiFunctions.LogActivity]         = "LogActivity",
        [ApiFunctions.SaveReadingProgress] = "SaveReadingProgress",
        [ApiFunctions.SetFavorite]         = "SetFavorite"
    };

    // The storefront must render for signed-out visitors, so these carry no auth headers.
    public static readonly HashSet<ApiFunctions> Anonymous = [ApiFunctions.GetAllBooks];
}
