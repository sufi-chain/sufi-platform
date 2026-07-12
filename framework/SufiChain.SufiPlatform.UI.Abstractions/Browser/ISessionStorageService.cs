namespace SufiChain.SufiPlatform.UI.Browser;

/// <summary>
/// Service for managing browser session storage via JavaScript interop.
/// </summary>
public interface ISessionStorageService
{
    /// <summary>
    /// Sets a value in session storage.
    /// </summary>
    ValueTask SetItemAsync(string key, string value);

    /// <summary>
    /// Gets a value from session storage.
    /// </summary>
    ValueTask<string?> GetItemAsync(string key);

    /// <summary>
    /// Removes a value from session storage.
    /// </summary>
    ValueTask RemoveItemAsync(string key);

    /// <summary>
    /// Clears all items from session storage.
    /// </summary>
    ValueTask ClearAsync();

    /// <summary>
    /// Gets the number of items in session storage.
    /// </summary>
    ValueTask<int> GetLengthAsync();

    /// <summary>
    /// Gets the key at the specified index.
    /// </summary>
    ValueTask<string?> GetKeyAsync(int index);
}
