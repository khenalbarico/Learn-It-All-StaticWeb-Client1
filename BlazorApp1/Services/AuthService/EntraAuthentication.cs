using System.Security.Claims;
using BlazorApp1.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorApp1.Services.AuthService;

public class EntraAuthentication : IAppAuthentication, IDisposable
{
    private readonly AuthenticationStateProvider _stateProvider;
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly NavigationManager _nav;

    public event Action? StateChanged;

    public EntraAuthentication(AuthenticationStateProvider stateProvider, IAccessTokenProvider tokenProvider, NavigationManager nav)
    {
        _stateProvider = stateProvider;
        _tokenProvider = tokenProvider;
        _nav = nav;
        _stateProvider.AuthenticationStateChanged += HandleAuthenticationStateChanged;
    }

    private void HandleAuthenticationStateChanged(Task<AuthenticationState> _)
        => StateChanged?.Invoke();

    public async Task<bool> IsAuthenticatedAsync()
    {
        var state = await _stateProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.IsAuthenticated == true;
    }

    public async Task<AuthResult> GetAuthAsync()
    {
        var tokenResult = await _tokenProvider.RequestAccessToken();

        if (!tokenResult.TryGetToken(out var token))
            throw new UnauthorizedAccessException("Your session has expired. Please sign in again.");

        var state = await _stateProvider.GetAuthenticationStateAsync();
        var user  = state.User;

        return new AuthResult
        {
            Token = token.Value,
            Uid   = FindClaim(user, "oid") ?? FindClaim(user, ClaimTypes.NameIdentifier) ?? "",
            Email = FindClaim(user, "email") ?? FindClaim(user, "preferred_username") ?? ""
        };
    }

    public void SignIn(string returnUrl)
    {
        var options = new InteractiveRequestOptions
        {
            Interaction = InteractionType.SignIn,
            ReturnUrl   = _nav.ToAbsoluteUri(returnUrl).AbsoluteUri
        };

        _nav.NavigateToLogin("authentication/login", options);
    }

    public void SignOut(string returnUrl)
        => _nav.NavigateToLogout("authentication/logout", returnUrl);

    public void Dispose()
        => _stateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChanged;

    private static string? FindClaim(ClaimsPrincipal user, string claimType)
        => user.FindFirst(claimType)?.Value;
}
