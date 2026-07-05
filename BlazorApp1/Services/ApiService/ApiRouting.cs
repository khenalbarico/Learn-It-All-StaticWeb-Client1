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
        [ApiFunctions.StreamBookDocument] = "StreamBookDocument",
        [ApiFunctions.GetMyLibraryBooks]  = "GetMyLibraryBooks",
        [ApiFunctions.CreatePaymentIntent] = "CreatePaymentIntent",
        [ApiFunctions.GetPaymentStatus]     = "GetPaymentStatus",
        [ApiFunctions.LogActivity]          = "LogActivity",
        [ApiFunctions.UpdateUserKeywords]   = "UpdateUserKeywords",
        [ApiFunctions.SaveReadingProgress]  = "SaveReadingProgress",
        [ApiFunctions.SetFavorite]          = "SetFavorite",
        [ApiFunctions.AddToCart]            = "AddToCart",
        [ApiFunctions.RemoveFromCart]       = "RemoveFromCart",
    };
}
