using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.Processing;

public class VideoMetadata
{
    public TimeSpan Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = default!;
    public string VideoCodec { get; set; } = default!;
    public string AudioCodec { get; set; } = default!;
    public double FrameRate { get; set; }
    public long BitRate { get; set; }
}

public interface IVideoProcessor
{
    /// <summary>
    /// Extract metadata from video
    /// </summary>
    Task<VideoMetadata> GetMetadataAsync(
        Stream videoStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate thumbnail from video at specified time
    /// </summary>
    Task<byte[]> GenerateThumbnailAsync(
        Stream videoStream,
        TimeSpan? atTime = null,
        int width = 320,
        int height = 240,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if file is a valid video
    /// </summary>
    Task<bool> IsValidVideoAsync(
        Stream stream,
        string mimeType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get video format from stream
    /// </summary>
    Task<string> GetVideoFormatAsync(
        Stream stream,
        CancellationToken cancellationToken = default);
}

