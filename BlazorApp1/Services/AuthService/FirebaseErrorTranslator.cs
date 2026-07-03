using Firebase.Auth;

namespace BlazorApp1.Services.AuthService;

internal static class FirebaseErrorTranslator
{
    internal static string Translate(FirebaseAuthException ex)
    {
        var msg = ex.Message?.ToUpperInvariant() ?? string.Empty;

        if (msg.Contains("INVALID_LOGIN_CREDENTIALS") || msg.Contains("WRONG_PASSWORD") || msg.Contains("INVALID_PASSWORD"))
            return "Incorrect email or password. Please try again.";

        if (msg.Contains("EMAIL_NOT_FOUND") || msg.Contains("USER_NOT_FOUND") || msg.Contains("UNKNOWN_EMAIL"))
            return "No account found with that email address.";

        if (msg.Contains("EMAIL_EXISTS") || msg.Contains("ACCOUNT_EXISTS_WITH_DIFFERENT_CREDENTIAL"))
            return "An account with this email already exists. Try signing in instead.";

        if (msg.Contains("WEAK_PASSWORD"))
            return "Password is too weak. Please use at least 6 characters.";

        if (msg.Contains("INVALID_EMAIL"))
            return "That doesn't look like a valid email address.";

        if (msg.Contains("TOO_MANY_ATTEMPTS") || msg.Contains("TOO_MANY_REQUESTS"))
            return "Too many failed attempts. Please wait a moment and try again.";

        if (msg.Contains("USER_DISABLED"))
            return "This account has been disabled. Please contact support.";

        if (msg.Contains("OPERATION_NOT_ALLOWED"))
            return "This sign-in method is not currently enabled.";

        if (msg.Contains("TOKEN_EXPIRED") || msg.Contains("SESSION_EXPIRED"))
            return "Your session has expired. Please sign in again.";

        if (msg.Contains("MISSING_PASSWORD"))
            return "Please enter your password.";

        if (msg.Contains("MISSING_EMAIL"))
            return "Please enter your email address.";

        return "Authentication failed. Please check your details and try again.";
    }
}
