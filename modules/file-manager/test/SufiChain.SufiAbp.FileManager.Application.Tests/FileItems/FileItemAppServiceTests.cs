using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileTypes;
using Shouldly;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Validation;
using Xunit;

namespace SufiChain.SufiAbp.FileManager.Application.Tests.FileItems;

public class FileItemAppServiceTests : FileManagerApplicationTestBase<SufiAbpFileManagerApplicationTestModule>
{
    private readonly IFileItemAppService _fileItemAppService;

    public FileItemAppServiceTests()
    {
        _fileItemAppService = GetRequiredService<IFileItemAppService>();
    }

    [Fact]
    public async Task Should_Upload_File()
    {
        // Arrange
        var testImage = CreateTestImageData();
        var input = new UploadFileInput
        {
            FileName = "test-image.png",
            Content = testImage,
            MimeType = "image/png",
            AutoConfirm = true,
            Alt = "Test Image"
        };

        // Act
        var result = await _fileItemAppService.UploadAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.OriginalName.ShouldBe("test-image.png");
        result.MimeType.ShouldBe("image/png");
        result.FileType.ShouldBe(FileType.Image);
        result.Size.ShouldBe(testImage.Length);
        result.Alt.ShouldBe("Test Image");
    }

    [Fact]
    public async Task Should_Get_File_By_Id()
    {
        // Arrange
        var uploaded = await UploadTestFileAsync();

        // Act
        var result = await _fileItemAppService.GetAsync(uploaded.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(uploaded.Id);
        result.OriginalName.ShouldBe(uploaded.OriginalName);
    }

    [Fact]
    public async Task Should_Get_File_List()
    {
        // Arrange
        await UploadTestFileAsync("image1.png");
        await UploadTestFileAsync("image2.png");
        await UploadTestFileAsync("image3.png");

        // Act
        var result = await _fileItemAppService.GetListAsync(new GetFileListInput
        {
            MaxResultCount = 10
        });

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(3);
        result.Items.Count.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Should_Filter_Files_By_Type()
    {
        // Arrange
        await UploadTestFileAsync("image.png", FileType.Image);
        await UploadTestFileAsync("video.mp4", FileType.Video);

        // Act
        var result = await _fileItemAppService.GetListAsync(new GetFileListInput
        {
            FileType = FileType.Image,
            MaxResultCount = 100
        });

        // Assert
        result.Items.ShouldAllBe(x => x.FileType == FileType.Image);
    }

    [Fact]
    public async Task Should_Search_Files_By_Keyword()
    {
        // Arrange
        await UploadTestFileAsync("product-main.png");
        await UploadTestFileAsync("product-gallery-1.png");
        await UploadTestFileAsync("user-avatar.png");

        // Act
        var result = await _fileItemAppService.GetListAsync(new GetFileListInput
        {
            Keyword = "product",
            MaxResultCount = 100
        });

        // Assert
        result.Items.ShouldAllBe(x => x.OriginalName.Contains("product", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_Sort_Files()
    {
        // Arrange
        await UploadTestFileAsync("a-file.png");
        await UploadTestFileAsync("b-file.png");
        await UploadTestFileAsync("c-file.png");

        // Act
        var resultAsc = await _fileItemAppService.GetListAsync(new GetFileListInput
        {
            Sorting = "OriginalName ASC",
            MaxResultCount = 100
        });

        var resultDesc = await _fileItemAppService.GetListAsync(new GetFileListInput
        {
            Sorting = "OriginalName DESC",
            MaxResultCount = 100
        });

        // Assert
        resultAsc.Items.First().OriginalName.ShouldContain("a-file");
        resultDesc.Items.First().OriginalName.ShouldContain("c-file");
    }

    [Fact]
    public async Task Should_Delete_File()
    {
        // Arrange
        var uploaded = await UploadTestFileAsync();

        // Act
        await _fileItemAppService.DeleteAsync(uploaded.Id);

        // Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(async () =>
        {
            await _fileItemAppService.GetAsync(uploaded.Id);
        });
    }

    [Fact]
    public async Task Should_Update_Metadata()
    {
        // Arrange
        var uploaded = await UploadTestFileAsync();

        // Act
        var updated = await _fileItemAppService.UpdateMetadataAsync(uploaded.Id, new UpdateFileMetadataInput
        {
            Alt = "Updated Alt Text",
            Tags = new[] { "tag1", "tag2" }
        });

        // Assert
        updated.Alt.ShouldBe("Updated Alt Text");
        updated.Tags.ShouldContain("tag1");
        updated.Tags.ShouldContain("tag2");
    }

    [Fact]
    public async Task Should_Get_Storage_Quota()
    {
        // Arrange
        await UploadTestFileAsync();
        await UploadTestFileAsync();

        // Act
        var quota = await _fileItemAppService.GetStorageQuotaAsync();

        // Assert
        quota.ShouldNotBeNull();
        quota.UsedBytes.ShouldBeGreaterThan(0);
        quota.UsedMB.ShouldBeGreaterThan(0);
        quota.LimitMB.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Upload_Multiple_Files()
    {
        // Arrange
        var files = new[]
        {
            new FileInput
            {
                FileName = "file1.png",
                Content = CreateTestImageData(),
                MimeType = "image/png"
            },
            new FileInput
            {
                FileName = "file2.png",
                Content = CreateTestImageData(),
                MimeType = "image/png"
            }
        };

        var input = new UploadMultipleFileInput
        {
            Files = files,
            AutoConfirm = true
        };

        // Act
        var result = await _fileItemAppService.UploadMultipleAsync(input);

        // Assert
        result.Items.Count.ShouldBe(2);
        result.Items.ShouldAllBe(x => x.Id != Guid.Empty);
    }

    [Fact]
    public async Task Should_Confirm_Temporary_File()
    {
        // Arrange
        var uploaded = await UploadTestFileAsync(autoConfirm: false);
        uploaded.IsTemp.ShouldBeTrue();

        // Act
        var confirmed = await _fileItemAppService.ConfirmAsync(uploaded.Id);

        // Assert
        confirmed.IsTemp.ShouldBeFalse();
        confirmed.BlobName.ShouldNotContain("temp");
    }

    [Fact]
    public async Task Should_Get_Download_Url()
    {
        // Arrange
        var uploaded = await UploadTestFileAsync();

        // Act
        var url = await _fileItemAppService.GetDownloadUrlAsync(uploaded.Id);

        // Assert
        url.ShouldNotBeNullOrEmpty();
        url.ShouldContain(uploaded.Id.ToString());
    }

    [Fact]
    public async Task Should_Delete_Many()
    {
        // Arrange
        var file1 = await UploadTestFileAsync();
        var file2 = await UploadTestFileAsync();
        var file3 = await UploadTestFileAsync();

        // Act
        await _fileItemAppService.DeleteManyAsync(new[] { file1.Id, file2.Id, file3.Id });

        // Assert
        var list = await _fileItemAppService.GetListAsync(new GetFileListInput { MaxResultCount = 100 });
        list.Items.ShouldNotContain(x => x.Id == file1.Id);
        list.Items.ShouldNotContain(x => x.Id == file2.Id);
        list.Items.ShouldNotContain(x => x.Id == file3.Id);
    }

    #region Helper Methods

    private byte[] CreateTestImageData()
    {
        // Create a minimal PNG image (1x1 pixel)
        return new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, // IHDR chunk length
            0x49, 0x48, 0x44, 0x52, // IHDR chunk type
            0x00, 0x00, 0x00, 0x01, // Width: 1
            0x00, 0x00, 0x00, 0x01, // Height: 1
            0x08, 0x02, 0x00, 0x00, 0x00, // Bit depth, color type, etc.
            0x90, 0x77, 0x53, 0xDE, // CRC
            0x00, 0x00, 0x00, 0x0C, // IDAT chunk length
            0x49, 0x44, 0x41, 0x54, // IDAT chunk type
            0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, 0x00,
            0x03, 0x01, 0x01, 0x00, // Image data
            0x18, 0xDD, 0x8D, 0xB4, // CRC
            0x00, 0x00, 0x00, 0x00, // IEND chunk length
            0x49, 0x45, 0x4E, 0x44, // IEND chunk type
            0xAE, 0x42, 0x60, 0x82  // CRC
        };
    }

    private async Task<FileItemDto> UploadTestFileAsync(
        string fileName = "test.png",
        FileType fileType = FileType.Image,
        bool autoConfirm = true)
    {
        var input = new UploadFileInput
        {
            FileName = fileName,
            Content = CreateTestImageData(),
            MimeType = fileType == FileType.Image ? "image/png" : "video/mp4",
            AutoConfirm = autoConfirm
        };

        return await _fileItemAppService.UploadAsync(input);
    }

    #endregion
}
