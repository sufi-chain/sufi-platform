namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Result of a storage connection test.
/// </summary>
public class TestStorageConnectionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
