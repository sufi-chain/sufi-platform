using System;
using System.IO;
using System.Threading.Tasks;
using SufiChain.SufiAbp.FileManager.Processing;
using Shouldly;
using Xunit;

namespace SufiChain.SufiAbp.FileManager.Application.Tests.Processing;

public class VideoProcessorTests : FileManagerApplicationTestBase<FileManagerApplicationTestModule>
{
    private readonly IVideoProcessor _videoProcessor;

    public VideoProcessorTests()
    {
        _videoProcessor = GetRequiredService<IVideoProcessor>();
    }

    [Fact(Skip = "Requires FFMpeg installed and test video file")]
    public async Task Should_Get_Video_Metadata()
    {
        // Arrange
        var testVideoPath = GetTestVideoPath();
        using var videoStream = File.OpenRead(testVideoPath);

        // Act
        var metadata = await _videoProcessor.GetMetadataAsync(videoStream);

        // Assert
        metadata.ShouldNotBeNull();
        metadata.Duration.ShouldBeGreaterThan(TimeSpan.Zero);
        metadata.Width.ShouldBeGreaterThan(0);
        metadata.Height.ShouldBeGreaterThan(0);
        metadata.Format.ShouldNotBeNullOrEmpty();
        metadata.VideoCodec.ShouldNotBeNullOrEmpty();
    }

    [Fact(Skip = "Requires FFMpeg installed and test video file")]
    public async Task Should_Generate_Video_Thumbnail()
    {
        // Arrange
        var testVideoPath = GetTestVideoPath();
        using var videoStream = File.OpenRead(testVideoPath);

        // Act
        var thumbnail = await _videoProcessor.GenerateThumbnailAsync(
            videoStream,
            atTime: TimeSpan.FromSeconds(5),
            width: 320,
            height: 240);

        // Assert
        thumbnail.ShouldNotBeNull();
        thumbnail.Length.ShouldBeGreaterThan(0);
    }

    [Fact(Skip = "Requires FFMpeg installed and test video file")]
    public async Task Should_Generate_Thumbnail_At_Default_Time()
    {
        // Arrange
        var testVideoPath = GetTestVideoPath();
        using var videoStream = File.OpenRead(testVideoPath);

        // Act
        var thumbnail = await _videoProcessor.GenerateThumbnailAsync(
            videoStream,
            atTime: null, // Should use default (5 seconds or half duration)
            width: 320,
            height: 240);

        // Assert
        thumbnail.ShouldNotBeNull();
        thumbnail.Length.ShouldBeGreaterThan(0);
    }

    [Fact(Skip = "Requires FFMpeg installed and test video file")]
    public async Task Should_Validate_Valid_Video()
    {
        // Arrange
        var testVideoPath = GetTestVideoPath();
        using var videoStream = File.OpenRead(testVideoPath);

        // Act
        var isValid = await _videoProcessor.IsValidVideoAsync(videoStream, "video/mp4");

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Reject_Invalid_Video()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        using var stream = new MemoryStream(invalidData);

        // Act
        var isValid = await _videoProcessor.IsValidVideoAsync(stream, "video/mp4");

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact(Skip = "Requires FFMpeg installed and test video file")]
    public async Task Should_Get_Video_Format()
    {
        // Arrange
        var testVideoPath = GetTestVideoPath();
        using var videoStream = File.OpenRead(testVideoPath);

        // Act
        var format = await _videoProcessor.GetVideoFormatAsync(videoStream);

        // Assert
        format.ShouldNotBeNullOrEmpty();
    }

    [Fact(Skip = "Requires FFMpeg installed and test video file")]
    public async Task Metadata_Should_Include_Audio_Codec()
    {
        // Arrange
        var testVideoPath = GetTestVideoPath();
        using var videoStream = File.OpenRead(testVideoPath);

        // Act
        var metadata = await _videoProcessor.GetMetadataAsync(videoStream);

        // Assert
        // Audio codec might be "none" for videos without audio
        metadata.AudioCodec.ShouldNotBeNullOrEmpty();
    }

    [Fact(Skip = "Requires FFMpeg installed and test video file")]
    public async Task Should_Generate_Multiple_Thumbnails_From_Same_Video()
    {
        // Arrange
        var testVideoPath = GetTestVideoPath();

        // Act
        byte[] thumbnail1;
        byte[] thumbnail2;

        using (var videoStream = File.OpenRead(testVideoPath))
        {
            thumbnail1 = await _videoProcessor.GenerateThumbnailAsync(
                videoStream,
                atTime: TimeSpan.FromSeconds(5),
                width: 320,
                height: 240);
        }

        using (var videoStream = File.OpenRead(testVideoPath))
        {
            thumbnail2 = await _videoProcessor.GenerateThumbnailAsync(
                videoStream,
                atTime: TimeSpan.FromSeconds(10),
                width: 320,
                height: 240);
        }

        // Assert
        thumbnail1.ShouldNotBeNull();
        thumbnail2.ShouldNotBeNull();
        thumbnail1.ShouldNotBe(thumbnail2); // Different timestamps should produce different images
    }

    #region Helper Methods

    private string GetTestVideoPath()
    {
        // In a real test, you would:
        // 1. Have a small test video file in test resources
        // 2. Or generate a simple test video programmatically
        // 3. Or download a small test video during test setup
        
        var testVideoPath = Path.Combine(AppContext.BaseDirectory, "TestAssets", "test-video.mp4");
        
        if (!File.Exists(testVideoPath))
        {
            throw new FileNotFoundException(
                "Test video file not found. Please add a test video to TestAssets/test-video.mp4", 
                testVideoPath);
        }

        return testVideoPath;
    }

    #endregion
}

