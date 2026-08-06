using Microsoft.JSInterop;
using SufiChain.SufiPlatform.UI.Browser;

namespace SufiChain.SufiPlatform.UI.Blazor.Browser;

/// <summary>
/// Implementation of ILocalStorageService using JavaScript interop.
/// </summary>
public class BrowserLocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserLocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask SetItemAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async ValueTask<string?> GetItemAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public async ValueTask RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async ValueTask ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.clear");
    }

    public async ValueTask<int> GetLengthAsync()
    {
        return await _jsRuntime.InvokeAsync<int>("eval", "localStorage.length");
    }

    public async ValueTask<string?> GetKeyAsync(int index)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.key", index);
    }
}
