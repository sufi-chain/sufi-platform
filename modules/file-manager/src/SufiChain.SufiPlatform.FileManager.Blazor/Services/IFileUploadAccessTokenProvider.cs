namespace SufiChain.SufiPlatform.FileManager.Blazor.Services;

/// <summary>
/// Interface for accessing authentication tokens for file uploads.
/// Implementations are provided by hosting-specific modules:
/// <list type="bullet">
/// <item><description>Blazor Server: Uses IHttpContextAccessor (SufiChain.SufiPlatform.FileManager.Blazor.Server)</description></item>
/// <item><description>Blazor WebAssembly: Uses IAccessTokenProvider (SufiChain.SufiPlatform.FileManager.Blazor.WebAssembly)</description></item>
/// </list>
/// </summary>
public interface IFileUploadAccessTokenProvider
{
    /// <summary>
    /// Gets the access token for the current user, or null if not available.
    /// </summary>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    /// Gets the access token or throws <see cref="UnauthorizedAccessException"/> if not available.
    /// Use this when authentication is required for the operation.
    /// </summary>
    Task<string> GetRequiredAccessTokenAsync();
}
