using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.BlobStoring.S3Provider;
using SufiChain.SufiPlatform.FileManager.Caching;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Integration;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

public class FileStorageIntegrationServiceTests
{
    [Fact]
    public async Task GetAsync_Should_Read_Repository_Without_FileItemAppService()
    {
        var fileId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var file = new FileItem(
            fileId,
            tenantId,
            "photo.webp",
            "photo.webp",
            "blobs/photo.webp",
            "image/webp",
            33532,
            FileType.Image,
            "Chat.Attachments");
        file.AssociateWith("Chat.Session", sessionId);
        file.Confirm();

        var appService = Substitute.For<IFileItemAppService>();
        var tokens = Substitute.For<IFileAccessTokenService>();
        var repository = Substitute.For<IFileItemRepository>();
        repository.GetAsync(fileId, Arg.Any<CancellationToken>()).Returns(file);

        var sut = new FileStorageIntegrationService(
            appService,
            tokens,
            repository,
            CreateBlobAccess());

        var result = await sut.GetAsync(fileId);

        result.Id.ShouldBe(fileId);
        result.FileName.ShouldBe("photo.webp");
        result.MimeType.ShouldBe("image/webp");
        result.SizeInBytes.ShouldBe(33532);
        result.StructureKey.ShouldBe("Chat.Attachments");
        result.EntityType.ShouldBe("Chat.Session");
        result.EntityId.ShouldBe(sessionId);
        result.TenantId.ShouldBe(tenantId);
        await appService.DidNotReceive().GetAsync(fileId);
    }

    [Fact]
    public async Task GetContentAsync_Should_Use_Integration_Blob_Access_Without_FileItemAppService()
    {
        var fileId = Guid.NewGuid();
        var bytes = new byte[] { 1, 2, 3, 4 };
        var appService = Substitute.For<IFileItemAppService>();
        var tokens = Substitute.For<IFileAccessTokenService>();
        var repository = Substitute.For<IFileItemRepository>();
        var blobAccess = CreateBlobAccess();
        blobAccess.GetContentForIntegrationAsync(fileId)
            .Returns(new FileContentResultDto
            {
                Content = new FileContentDto
                {
                    Content = bytes,
                    FileName = "photo.webp",
                    MimeType = "image/webp"
                }
            });

        var sut = new FileStorageIntegrationService(
            appService,
            tokens,
            repository,
            blobAccess);

        var result = await sut.GetContentAsync(fileId);

        result.Id.ShouldBe(fileId);
        result.FileName.ShouldBe("photo.webp");
        result.MimeType.ShouldBe("image/webp");
        result.Content.ShouldBe(bytes);
        await appService.DidNotReceive().GetDownloadContentAsync(fileId, Arg.Any<string?>());
    }

    [Fact]
    public async Task GetContentAsync_Should_Forbid_When_Blob_Access_Is_Forbidden()
    {
        var fileId = Guid.NewGuid();
        var blobAccess = CreateBlobAccess();
        blobAccess.GetContentForIntegrationAsync(fileId)
            .Returns(new FileContentResultDto { IsForbidden = true });

        var sut = new FileStorageIntegrationService(
            Substitute.For<IFileItemAppService>(),
            Substitute.For<IFileAccessTokenService>(),
            Substitute.For<IFileItemRepository>(),
            blobAccess);

        await Should.ThrowAsync<Volo.Abp.Authorization.AbpAuthorizationException>(
            () => sut.GetContentAsync(fileId));
    }

    private static FileItemBlobAccessService CreateBlobAccess()
    {
        return Substitute.For<FileItemBlobAccessService>(
            Substitute.For<IFileItemRepository>(),
            Substitute.For<IStructureCache>(),
            Substitute.For<IStructureBlobContainerProvider>(),
            Substitute.For<IFileAccessTokenService>(),
            Substitute.For<IS3PublicBlobUrlProvider>(),
            Substitute.For<IS3PresignedUrlProvider>(),
            Options.Create(new FileManagerOptions()),
            Substitute.For<ILogger<FileItemBlobAccessService>>(),
            Substitute.For<ICurrentTenant>(),
            Substitute.For<ICurrentUser>(),
            Substitute.For<IDataFilter>());
    }
}
