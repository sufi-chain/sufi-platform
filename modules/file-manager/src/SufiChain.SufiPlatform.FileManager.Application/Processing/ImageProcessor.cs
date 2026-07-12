using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.FileManager.Processing;

public class ImageProcessor : IImageProcessor, ITransientDependency
{
    private readonly ILogger<ImageProcessor> _logger;

    public ImageProcessor(ILogger<ImageProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenerateThumbnailAsync(
        byte[] imageData,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var inStream = new MemoryStream(imageData);
            using var image = await Image.LoadAsync(inStream, cancellationToken);

            // Auto-orient based on EXIF data
            image.Mutate(x => x.AutoOrient());

            // Calculate dimensions maintaining aspect ratio
            var ratioX = (double)width / image.Width;
            var ratioY = (double)height / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(newWidth, newHeight),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));

            using var outStream = new MemoryStream();
            
            // Save as WebP for optimal size
            var encoder = new WebpEncoder
            {
                Quality = 75,
                FileFormat = WebpFileFormatType.Lossy
            };

            await image.SaveAsync(outStream, encoder, cancellationToken);
            return outStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate thumbnail");
            throw;
        }
    }

    public async Task<(byte[] data, string mimeType, string extension)> ConvertToWebPAsync(
        byte[] imageData,
        int quality = 80,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("WebP conversion starting. Input size: {InputSize} bytes", imageData?.Length ?? 0);
            
            if (imageData == null || imageData.Length == 0)
            {
                _logger.LogWarning("WebP conversion received null or empty input data");
                throw new ArgumentException("Image data is null or empty", nameof(imageData));
            }
            
            using var inStream = new MemoryStream(imageData);
            _logger.LogDebug("Loading image from stream...");
            
            using var image = await Image.LoadAsync(inStream, cancellationToken);
            _logger.LogInformation("Image loaded successfully. Dimensions: {Width}x{Height}, Format: {Format}", 
                image.Width, image.Height, image.Metadata.DecodedImageFormat?.Name ?? "unknown");

            // Auto-orient based on EXIF
            image.Mutate(x => x.AutoOrient());

            using var outStream = new MemoryStream();
            
            var encoder = new WebpEncoder
            {
                Quality = quality,
                FileFormat = WebpFileFormatType.Lossy,
                Method = WebpEncodingMethod.BestQuality
            };

            _logger.LogDebug("Encoding to WebP with quality {Quality}...", quality);
            await image.SaveAsync(outStream, encoder, cancellationToken);
            
            var result = outStream.ToArray();
            _logger.LogInformation("WebP conversion completed. Output size: {OutputSize} bytes (ratio: {Ratio:P1})", 
                result.Length, (double)result.Length / imageData.Length);
            
            return (result, "image/webp", ".webp");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert image to WebP. Input size was: {InputSize} bytes", imageData?.Length ?? 0);
            throw;
        }
    }

    public async Task<(int width, int height)> GetDimensionsAsync(
        byte[] imageData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var inStream = new MemoryStream(imageData);
            var info = await Image.IdentifyAsync(inStream, cancellationToken);
            
            return info != null ? (info.Width, info.Height) : (0, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get image dimensions");
            return (0, 0);
        }
    }

    public async Task<byte[]> ResizeAsync(
        byte[] imageData,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var inStream = new MemoryStream(imageData);
            using var image = await Image.LoadAsync(inStream, cancellationToken);

            // Auto-orient
            image.Mutate(x => x.AutoOrient());

            // Check if resize is needed
            if (image.Width <= maxWidth && image.Height <= maxHeight)
            {
                return imageData; // No resize needed
            }

            // Calculate new dimensions
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(newWidth, newHeight),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));

            using var outStream = new MemoryStream();
            
            // Save in original format
            await image.SaveAsync(outStream, image.Metadata.DecodedImageFormat!, cancellationToken);
            
            return outStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resize image");
            throw;
        }
    }

    public async Task<bool> IsValidImageAsync(
        byte[] data,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var inStream = new MemoryStream(data);
            var info = await Image.IdentifyAsync(inStream, cancellationToken);
            return info != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetImageFormatAsync(
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var inStream = new MemoryStream(data);
            var info = await Image.IdentifyAsync(inStream, cancellationToken);
            
            return info?.Metadata.DecodedImageFormat?.Name ?? "unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get image format");
            return "unknown";
        }
    }
}

