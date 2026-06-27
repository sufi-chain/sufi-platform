using System.Text.Json;
using Microsoft.JSInterop;
using SufiChain.SufiAbp.UI.Browser;

namespace SufiChain.SufiAbp.UI.Blazor.Browser;

/// <summary>
/// Browser session storage access with Blazor Server-safe JS interop handling.
/// </summary>
public class BrowserSessionStorageService : ISessionStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserSessionStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask SetItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async ValueTask<string?> GetItemAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", key);
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async ValueTask RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async ValueTask ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.clear");
        }
        catch (JSDisconnectedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async ValueTask<int> GetLengthAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<int>("eval", "sessionStorage.length");
        }
        catch (JSDisconnectedException)
        {
            return 0;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    public async ValueTask<string?> GetKeyAsync(int index)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("sessionStorage.key", index);
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async ValueTask<T?> GetAsync<T>(string key)
    {
        var json = await GetItemAsync(key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public ValueTask SetAsync<T>(string key, T value)
    {
        return SetItemAsync(key, JsonSerializer.Serialize(value));
    }

    public ValueTask RemoveAsync(string key)
    {
        return RemoveItemAsync(key);
    }
}
