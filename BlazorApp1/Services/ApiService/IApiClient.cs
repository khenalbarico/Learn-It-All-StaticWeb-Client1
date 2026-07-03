using BlazorApp1.Models;

namespace BlazorApp1.Services.ApiService;

public interface IApiClient
{
    Task<T> GetAsync<T>(ApiFunctions apiFunction, object? payload = null);
    Task<T> SubmitAsync<T>(ApiFunctions apiFunction, object? payload = null);
    Task SubmitAsync(ApiFunctions apiFunction, object? payload = null);
    Task<byte[]> GetBytesAsync(ApiFunctions apiFunction, object? payload = null);
}
