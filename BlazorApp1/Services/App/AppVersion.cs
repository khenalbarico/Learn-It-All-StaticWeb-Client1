using System.Reflection;

namespace BlazorApp1.Services.App;

public static class AppVersion
{
    public static string Display { get; } = BuildDisplayString();

    private static string BuildDisplayString()
    {
        var informational = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

        var version = informational.Split('+')[0];
        return string.IsNullOrWhiteSpace(version) ? "dev" : $"v{version}";
    }
}
