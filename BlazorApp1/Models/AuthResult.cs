namespace BlazorApp1.Models;

// Mirrors what the Firebase JS SDK hands back for the signed-in user. Token is the
// ID token; ExpiresAt is that token's own expiry, which drives when the API needs to
// re-verify rather than us guessing a refresh interval.
public class AuthResult
{
    public string   Token       { get; set; } = string.Empty;
    public string   Uid         { get; set; } = string.Empty;
    public string?  Email       { get; set; }
    public string?  DisplayName { get; set; }
    public string?  PhotoUrl    { get; set; }
    public DateTime ExpiresAt   { get; set; }
}

// What the API returns from VerifyAuth once it has checked the token with Firebase Admin.
public class VerifiedAuth
{
    public string   Uid       { get; set; } = string.Empty;
    public string?  Email     { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public record ProfileHint(string? FirstName, string? LastName, string? PhoneNumber);
