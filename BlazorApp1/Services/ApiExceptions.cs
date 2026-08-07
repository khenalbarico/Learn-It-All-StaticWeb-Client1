namespace BlazorApp1.Services;

// The API distinguishes between "you already own this", "slow down" and "something broke",
// but the client used to collapse all three into one failure. These carry the distinction
// far enough for the UI to say something true — telling a buyer who just paid that nothing
// was charged is worse than saying nothing at all.

public class AlreadyOwnedException(string message) : Exception(message);

public class TooManyAttemptsException(string message, int retryAfterSeconds) : Exception(message)
{
    public int RetryAfterSeconds { get; } = retryAfterSeconds;
}

public class PaymentProviderUnavailableException(string message) : Exception(message);
