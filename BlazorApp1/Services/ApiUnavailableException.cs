namespace BlazorApp1.Services;

public class ApiUnavailableException(string message, Exception inner) : Exception(message, inner);
