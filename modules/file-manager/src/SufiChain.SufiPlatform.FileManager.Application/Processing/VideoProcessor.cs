using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.FileManager.Processing;

public class VideoProcessor : IVideoProcessor, ITransientDependency
{
    private readonly ILogger<VideoProcessor> _logger;

    public VideoProcessor(ILogger<VideoProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<VideoMetadata> GetMetadataAsync(
        Stream videoStream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                // Save stream to temp file for FFMpeg
                using (var fileStream = File.Create(tempFile))
                {
                    videoStream.Position = 0;
                    await videoStream.CopyToAsync(fileStream, cancellationToken);
                }

                var mediaInfo = await FFProbe.AnalyseAsync(tempFile, null, cancellationToken);

                return new VideoMetadata
                {
                    Duration = mediaInfo.Duration,
                    Width = mediaInfo.PrimaryVideoStream?.Width ?? 0,
                    Height = mediaInfo.PrimaryVideoStream?.Height ?? 0,
                    Format = mediaInfo.Format.FormatName,
                    VideoCodec = mediaInfo.PrimaryVideoStream?.CodecName ?? "unknown",
                    AudioCodec = mediaInfo.PrimaryAudioStream?.CodecName ?? "none",
                    FrameRate = mediaInfo.PrimaryVideoStream?.FrameRate ?? 0,
                    BitRate = (long)mediaInfo.Format.BitRate
                };
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract video metadata");
            throw;
        }
    }

    public async Task<byte[]> GenerateThumbnailAsync(
        Stream videoStream,
        TimeSpan? atTime = null,
        int width = 320,
        int height = 240,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tempFile = Path.GetTempFileName();
            var outputFile = Path.GetTempFileName() + ".jpg";
            
            try
            {
                // Save stream to temp file
                using (var fileStream = File.Create(tempFile))
                {
                    videoStream.Position = 0;
                    await videoStream.CopyToAsync(fileStream, cancellationToken);
                }

                // Get video info to determine capture time if not specified
                var mediaInfo = await FFProbe.AnalyseAsync(tempFile, null, cancellationToken);
                var captureTime = atTime ?? TimeSpan.FromSeconds(Math.Min(5, mediaInfo.Duration.TotalSeconds / 2));

                // Generate thumbnail
                await FFMpeg.SnapshotAsync(
                    tempFile,
                    outputFile,
                    new System.Drawing.Size(width, height),
                    captureTime);

                // Read thumbnail bytes
                var thumbnailBytes = await File.ReadAllBytesAsync(outputFile, cancellationToken);
                return thumbnailBytes;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate video thumbnail");
            throw;
        }
    }

    public async Task<bool> IsValidVideoAsync(
        Stream stream,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fileStream = File.Create(tempFile))
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(fileStream, cancellationToken);
                }

                var mediaInfo = await FFProbe.AnalyseAsync(tempFile, null, cancellationToken);
                return mediaInfo.PrimaryVideoStream != null;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetVideoFormatAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fileStream = File.Create(tempFile))
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(fileStream, cancellationToken);
                }

                var mediaInfo = await FFProbe.AnalyseAsync(tempFile, null, cancellationToken);
                return mediaInfo.Format.FormatName;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get video format");
            return "unknown";
        }
    }
}

