using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.FileManager.Processing;

public interface IImageProcessor
{
    /// <summary>
    /// Generate a thumbnail from an image
    /// </summary>
    Task<byte[]> GenerateThumbnailAsync(
        byte[] imageData, 
        int width, 
        int height,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convert image to WebP format
    /// </summary>
    Task<(byte[] data, string mimeType, string extension)> ConvertToWebPAsync(
        byte[] imageData,
        int quality = 80,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get image dimensions
    /// </summary>
    Task<(int width, int height)> GetDimensionsAsync(
        byte[] imageData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resize image maintaining aspect ratio
    /// </summary>
    Task<byte[]> ResizeAsync(
        byte[] imageData,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if data is a valid image
    /// </summary>
    Task<bool> IsValidImageAsync(
        byte[] data,
        string mimeType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get image format from bytes
    /// </summary>
    Task<string> GetImageFormatAsync(
        byte[] data,
        CancellationToken cancellationToken = default);
}

