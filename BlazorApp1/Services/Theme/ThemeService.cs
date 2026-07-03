using BlazorApp1.Services.App;

namespace BlazorApp1.Services.Theme;

public enum AppTheme
{
    Light,
    Dark
}

public class ThemeService(JsInteropService _jsInterop)
{
    private const string StorageKey = "theme";

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        var stored = await _jsInterop.GetItemAsync(StorageKey);

        if (stored == "dark")
        {
            CurrentTheme = AppTheme.Dark;
        }
        else if (stored == "light")
        {
            CurrentTheme = AppTheme.Light;
        }
        else
        {
            var prefersDark = await _jsInterop.GetPreferredColorSchemeIsDarkAsync();
            CurrentTheme = prefersDark ? AppTheme.Dark : AppTheme.Light;
        }

        await _jsInterop.SetThemeAttributeAsync(CurrentTheme == AppTheme.Dark ? "dark" : "light");
    }

    public async Task ToggleAsync()
    {
        CurrentTheme = CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        var value = CurrentTheme == AppTheme.Dark ? "dark" : "light";

        await _jsInterop.SetItemAsync(StorageKey, value);
        await _jsInterop.SetThemeAttributeAsync(value);

        OnChange?.Invoke();
    }
}
