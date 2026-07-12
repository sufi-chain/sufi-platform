namespace SufiChain.SufiPlatform.UI.BlockUi;

/// <summary>
/// Service for blocking the UI during long-running operations.
/// </summary>
public interface IBlockUiService
{
    /// <summary>
    /// Blocks the UI, optionally targeting specific selectors.
    /// </summary>
    /// <param name="selectors">CSS selectors to block. Null blocks the entire page.</param>
    /// <param name="busy">Whether to show a busy indicator.</param>
    Task BlockAsync(string? selectors = null, bool busy = false);

    /// <summary>
    /// Unblocks the UI.
    /// </summary>
    Task UnblockAsync();
}
