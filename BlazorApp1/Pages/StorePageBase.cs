using BlazorApp1.Models;
using BlazorApp1.Services;
using BlazorApp1.Services.App;
using Microsoft.AspNetCore.Components;

namespace BlazorApp1.Pages;

// Shared by the storefront and the catalogue: load the (cached) book list, and route a
// Buy Now press either into checkout or into sign-in, depending on who's pressing it.
public abstract class StorePageBase : ComponentBase, IDisposable
{
    [Inject] protected IAppService      AppService { get; set; } = default!;
    [Inject] protected AuthSessionState AuthState  { get; set; } = default!;
    [Inject] protected NavigationManager Nav       { get; set; } = default!;

    protected List<Book> Books = [];
    protected Book?      BuyingBook;
    protected string?    LoadError;
    protected bool       Loading = true;

    protected override async Task OnInitializedAsync()
    {
        AuthState.OnChange += OnStateChanged;
        SearchState.OnChange += OnStateChanged;
        await LoadBooksAsync();
    }

    protected void OnStateChanged() => InvokeAsync(StateHasChanged);

    protected async Task LoadBooksAsync()
    {
        Loading   = true;
        LoadError = null;

        try
        {
            Books = await AppService.GetAllBooks();
        }
        catch (ApiUnavailableException ex)
        {
            LoadError = ex.Message;
        }
        catch (Exception)
        {
            LoadError = "Could not load the book catalogue. Please try again.";
        }
        finally
        {
            Loading = false;
            StateHasChanged();
        }
    }

    protected bool IsOwned(Book book)
        => AuthState.CurrentUser?.Library.Any(l => l.Uid == book.Uid) == true;

    // A guest pressing Buy Now is sent to sign-in with the book remembered, so the
    // purchase resumes the moment they're authenticated.
    protected void HandleBuy(Book book)
    {
        if (!AuthState.IsAuthenticated)
        {
            AuthState.OpenSignInModal(book.Uid);
            return;
        }

        BuyingBook = book;
    }

    protected void HandleRead(Book book) => Nav.NavigateTo($"read/{book.Uid}");

    protected void CloseBuyModal() => BuyingBook = null;

    // Picks up a purchase that was interrupted by the sign-in detour.
    protected void ResumePendingPurchase()
    {
        if (!AuthState.IsAuthenticated || AuthState.PendingBookUid is null || BuyingBook is not null) return;

        var uid = AuthState.TakePendingBookUid();
        var book = Books.FirstOrDefault(b => b.Uid == uid);

        if (book is not null && !IsOwned(book))
            BuyingBook = book;
    }

    public virtual void Dispose()
    {
        AuthState.OnChange -= OnStateChanged;
        SearchState.OnChange -= OnStateChanged;
        GC.SuppressFinalize(this);
    }
}
