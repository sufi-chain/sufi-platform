using System;
using System.IO;
using System.Threading.Tasks;
using SufiChain.SufiAbp.FileManager.Processing;
using Shouldly;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace SufiChain.SufiAbp.FileManager.Application.Tests.Processing;

public class ImageProcessorTests : FileManagerApplicationTestBase<SufiAbpFileManagerApplicationTestModule>
{
    private readonly IImageProcessor _imageProcessor;

    public ImageProcessorTests()
    {
        _imageProcessor = GetRequiredService<IImageProcessor>();
    }

    [Fact]
    public async Task Should_Generate_Thumbnail()
    {
        // Arrange
        var testImage = CreateTestImage(800, 600);

        // Act
        var thumbnail = await _imageProcessor.GenerateThumbnailAsync(testImage, 200, 200);

        // Assert
        thumbnail.ShouldNotBeNull();
        thumbnail.Length.ShouldBeGreaterThan(0);
        thumbnail.Length.ShouldBeLessThan(testImage.Length); // Thumbnail should be smaller
    }

    [Fact]
    public async Task Should_Convert_To_WebP()
    {
        // Arrange
        var testImage = CreateTestImage(800, 600);

        // Act
        var (webpData, mimeType, extension) = await _imageProcessor.ConvertToWebPAsync(testImage, quality: 80);

        // Assert
        webpData.ShouldNotBeNull();
        webpData.Length.ShouldBeGreaterThan(0);
        mimeType.ShouldBe("image/webp");
        extension.ShouldBe(".webp");
    }

    [Fact]
    public async Task Should_Get_Image_Dimensions()
    {
        // Arrange
        var testImage = CreateTestImage(800, 600);

        // Act
        var (width, height) = await _imageProcessor.GetDimensionsAsync(testImage);

        // Assert
        width.ShouldBe(800);
        height.ShouldBe(600);
    }

    [Fact]
    public async Task Should_Resize_Large_Image()
    {
        // Arrange
        var testImage = CreateTestImage(2000, 1500);

        // Act
        var resized = await _imageProcessor.ResizeAsync(testImage, 1024, 768);

        // Assert
        resized.ShouldNotBeNull();
        resized.Length.ShouldBeLessThan(testImage.Length);

        var (width, height) = await _imageProcessor.GetDimensionsAsync(resized);
        width.ShouldBeLessThanOrEqualTo(1024);
        height.ShouldBeLessThanOrEqualTo(768);
    }

    [Fact]
    public async Task Should_Not_Resize_Small_Image()
    {
        // Arrange
        var testImage = CreateTestImage(500, 400);

        // Act
        var resized = await _imageProcessor.ResizeAsync(testImage, 1024, 768);

        // Assert
        resized.ShouldBe(testImage); // Should return original if already small enough
    }

    [Fact]
    public async Task Should_Maintain_Aspect_Ratio_When_Resizing()
    {
        // Arrange
        var testImage = CreateTestImage(1600, 1200); // 4:3 ratio

        // Act
        var resized = await _imageProcessor.ResizeAsync(testImage, 800, 800);

        // Assert
        var (width, height) = await _imageProcessor.GetDimensionsAsync(resized);
        
        // Should maintain 4:3 ratio, fit within 800x800
        var ratio = (double)width / height;
        ratio.ShouldBe(1600.0 / 1200.0, 0.01); // Allow small floating point difference
    }

    [Fact]
    public async Task Should_Validate_Valid_Image()
    {
        // Arrange
        var testImage = CreateTestImage(800, 600);

        // Act
        var isValid = await _imageProcessor.IsValidImageAsync(testImage, "image/png");

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Reject_Invalid_Image()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        // Act
        var isValid = await _imageProcessor.IsValidImageAsync(invalidData, "image/png");

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Get_Image_Format()
    {
        // Arrange
        var testImage = CreateTestImage(800, 600);

        // Act
        var format = await _imageProcessor.GetImageFormatAsync(testImage);

        // Assert
        format.ShouldNotBeNullOrEmpty();
        format.ToLower().ShouldBeOneOf("png", "jpeg", "jpg");
    }

    [Fact]
    public async Task WebP_Conversion_Should_Respect_Quality_Setting()
    {
        // Arrange
        var testImage = CreateTestImage(800, 600);

        // Act
        var (highQuality, _, _) = await _imageProcessor.ConvertToWebPAsync(testImage, quality: 95);
        var (lowQuality, _, _) = await _imageProcessor.ConvertToWebPAsync(testImage, quality: 20);

        // Assert
        highQuality.Length.ShouldBeGreaterThan(lowQuality.Length);
    }

    [Fact]
    public async Task Thumbnail_Should_Be_Smaller_Than_Original()
    {
        // Arrange
        var testImage = CreateTestImage(1920, 1080);

        // Act
        var thumbnail = await _imageProcessor.GenerateThumbnailAsync(testImage, 200, 200);

        // Assert
        var (thumbWidth, thumbHeight) = await _imageProcessor.GetDimensionsAsync(thumbnail);
        
        thumbWidth.ShouldBeLessThanOrEqualTo(200);
        thumbHeight.ShouldBeLessThanOrEqualTo(200);
        thumbnail.Length.ShouldBeLessThan(testImage.Length);
    }

    #region Helper Methods

    private byte[] CreateTestImage(int width, int height)
    {
        // Create a simple test image using ImageSharp
        using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height);
        
        // Fill with a color
        image.Mutate(x => x.BackgroundColor(SixLabors.ImageSharp.Color.Blue));

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    #endregion
}

