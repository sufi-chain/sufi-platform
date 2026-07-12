using System;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Generates and validates signed tokens for thumbnail/stream URLs.
/// Used when img/video elements load media (they don't send Authorization headers).
/// </summary>
public interface IFileAccessTokenService
{
    /// <summary>
    /// True when a signing secret is configured and protected URLs can include signed tokens.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Generates a signed token for the given file ID.
    /// </summary>
    string GenerateToken(Guid fileId);

    /// <summary>
    /// Attempts to generate a signed token for the given file ID.
    /// </summary>
    bool TryGenerateToken(Guid fileId, out string token);

    /// <summary>
    /// Validates the token and returns the file ID if valid.
    /// </summary>
    /// <returns>True if valid; false otherwise.</returns>
    bool TryValidateToken(string? token, out Guid fileId);
}
