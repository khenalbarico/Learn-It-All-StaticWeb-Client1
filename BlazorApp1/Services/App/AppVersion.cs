using System.Globalization;
using System.Reflection;

namespace BlazorApp1.Services.App;

public static class AppVersion
{
    public static string Display { get; } = BuildDisplayString();

    private static string BuildDisplayString()
    {
        var informational = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

        var parts = informational.Split("+build.");
        if (parts.Length != 2 || !DateTime.TryParseExact(parts[1], "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var built))
            return string.IsNullOrEmpty(informational) ? "dev" : informational;

        return $"v{parts[0]} · built {built:yyyy-MM-dd HH:mm} UTC";
    }
}
