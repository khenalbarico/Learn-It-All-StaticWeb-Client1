using Microsoft.JSInterop;

namespace BlazorApp1.Services.App;

public class JsInteropService(IJSRuntime _jsRuntime)
{
    private IJSObjectReference? _module;

    private async Task<IJSObjectReference> GetModuleAsync()
        => _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/appInterop.js");

    public async Task<string?> GetItemAsync(string key)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string?>("getItem", key);
    }

    public async Task SetItemAsync(string key, string value)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("setItem", key, value);
    }

    public async Task RemoveItemAsync(string key)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("removeItem", key);
    }

    public async Task SetThemeAttributeAsync(string theme)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("setThemeAttribute", theme);
    }

    public async Task<bool> GetPreferredColorSchemeIsDarkAsync()
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("prefersDarkColorScheme");
    }

    public async Task<int> LoadPdfAsync(string containerId, byte[] bytes)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<int>("loadPdf", containerId, bytes);
    }

    public async Task<int> GoToPdfPageAsync(string containerId, int pageNum)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<int>("goToPdfPage", containerId, pageNum);
    }

    public async Task<double> ZoomPdfAsync(string containerId, int delta)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<double>("zoomPdf", containerId, delta);
    }

    public async Task DisposePdfAsync(string containerId)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("disposePdf", containerId);
    }

    public async Task PushAdsbygoogleAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("pushAdsbygoogle");
    }
}
