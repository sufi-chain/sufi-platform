namespace SufiChain.SufiPlatform.UI.Browser;

/// <summary>
/// Service for managing browser local storage via JavaScript interop.
/// </summary>
public interface ILocalStorageService
{
    /// <summary>
    /// Sets a value in local storage.
    /// </summary>
    ValueTask SetItemAsync(string key, string value);

    /// <summary>
    /// Gets a value from local storage.
    /// </summary>
    ValueTask<string?> GetItemAsync(string key);

    /// <summary>
    /// Removes a value from local storage.
    /// </summary>
    ValueTask RemoveItemAsync(string key);

    /// <summary>
    /// Clears all items from local storage.
    /// </summary>
    ValueTask ClearAsync();

    /// <summary>
    /// Gets the number of items in local storage.
    /// </summary>
    ValueTask<int> GetLengthAsync();

    /// <summary>
    /// Gets the key at the specified index.
    /// </summary>
    ValueTask<string?> GetKeyAsync(int index);
}
