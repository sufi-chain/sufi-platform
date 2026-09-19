using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.BlobStoring.S3Provider;
using SufiChain.SufiPlatform.FileManager.AccessControl;
using SufiChain.SufiPlatform.FileManager.Caching;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Processing;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

public class FileUploadValidationSecurityTests
{
    private const string StructureKey = "Chat.Attachments";

    [Fact]
    public async Task ValidateUpload_Should_Reject_Path_Traversal_Without_Structure()
    {
        var sut = CreateSut();

        var result = await sut.ValidateUploadAsync("../secret.exe", "image/png", null, 10);

        result.IsValid.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("payload.php", "text/plain")]
    [InlineData("payload.exe", "application/octet-stream")]
    [InlineData("page.html", "text/html")]
    [InlineData("icon.svg", "image/svg+xml")]
    [InlineData("photo.png.php", "image/png")]
    public async Task ValidateUpload_Should_Reject_Blocked_Extensions_Even_When_Structure_Allows_Mime(string fileName, string mimeType)
    {
        var sut = CreateSut(allowedExtensions: "png,php,exe,html,svg", allowedMimeTypes: mimeType);

        var result = await sut.ValidateUploadAsync(fileName, mimeType, StructureKey, 32);

        result.IsValid.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("not allowed");
    }

    [Fact]
    public async Task ValidateUpload_Should_Reject_Mime_Spoof_When_Extension_Is_Not_In_Structure()
    {
        var sut = CreateSut(allowedExtensions: "png,jpg", allowedMimeTypes: "image/png,image/jpeg");

        var result = await sut.ValidateUploadAsync("malware.exe", "image/png", StructureKey, 64);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateUpload_Should_Reject_Oversized_File()
    {
        var sut = CreateSut(maxFileSize: 100);

        var result = await sut.ValidateUploadAsync("photo.png", "image/png", StructureKey, 101);

        result.IsValid.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("size");
    }

    [Fact]
    public async Task ValidateUpload_Should_Accept_Allowed_Image()
    {
        var sut = CreateSut();

        var result = await sut.ValidateUploadAsync("photo.png", "image/png", StructureKey, 64);

        result.IsValid.ShouldBeTrue();
    }

    private static FileItemAppService CreateSut(
        string allowedExtensions = "png,jpg,jpeg,webp,pdf",
        string allowedMimeTypes = "image/png,image/jpeg,image/webp,application/pdf",
        long maxFileSize = 1024 * 1024)
    {
        var cache = Substitute.For<IStructureCache>();
        cache.GetAsync(StructureKey, Arg.Any<CancellationToken>())
            .Returns(new StructureCacheEntry
            {
                Key = StructureKey,
                AllowedExtensions = allowedExtensions,
                AllowedMimeTypes = allowedMimeTypes,
                MaxFileSize = maxFileSize,
                AllowedFileTypes = FileType.Image | FileType.Document
            });

        return new FileItemAppService(
            Substitute.For<IFileItemRepository>(),
            Substitute.For<IFileStructureRepository>(),
            cache,
            Substitute.For<IFolderAppService>(),
            Substitute.For<IFileFolderRepository>(),
            Substitute.For<IFolderAccessResolver>(),
            Substitute.For<IUserFolderAccessContextProvider>(),
            Substitute.For<IStructureBlobContainerProvider>(),
            Substitute.For<IImageProcessor>(),
            Substitute.For<IVideoProcessor>(),
            Substitute.For<IFileBlobNameCalculator>(),
            Options.Create(new FileManagerOptions()),
            Substitute.For<ISettingProvider>(),
            Substitute.For<ILogger<FileItemAppService>>(),
            Substitute.For<IS3PublicBlobUrlProvider>(),
            Substitute.For<FileItemManager>(
                Substitute.For<IFileItemRepository>(),
                Substitute.For<Volo.Abp.EventBus.Distributed.IDistributedEventBus>(),
                Substitute.For<Volo.Abp.Timing.IClock>(),
                Substitute.For<Volo.Abp.Users.ICurrentUser>(),
                Substitute.For<SufiChain.SufiPlatform.Features.IFeatureChecker>()),
            Substitute.For<FileItemBlobAccessService>(
                Substitute.For<IFileItemRepository>(),
                cache,
                Substitute.For<IStructureBlobContainerProvider>(),
                Substitute.For<IFileAccessTokenService>(),
                Substitute.For<IS3PublicBlobUrlProvider>(),
                Substitute.For<IS3PresignedUrlProvider>(),
                Options.Create(new FileManagerOptions()),
                Substitute.For<ILogger<FileItemBlobAccessService>>(),
                Substitute.For<Volo.Abp.MultiTenancy.ICurrentTenant>(),
                Substitute.For<Volo.Abp.Users.ICurrentUser>(),
                Substitute.For<Volo.Abp.Data.IDataFilter>()),
            Substitute.For<IFileStorageQuotaGuard>(),
            Substitute.For<IFileManagerStoragePolicyProvider>());
    }
}
