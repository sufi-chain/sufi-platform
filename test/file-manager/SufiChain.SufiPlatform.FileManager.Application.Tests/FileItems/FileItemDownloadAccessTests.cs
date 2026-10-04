using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.BlobStoring.S3Provider;
using SufiChain.SufiPlatform.FileManager.Caching;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.BlobStoring;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileItems;

public class FileItemDownloadAccessTests
{
    private readonly byte[] _bytes = [9, 8, 7];

    [Fact]
    public async Task Authenticated_caller_can_download_a_public_file_outside_the_current_tenant()
    {
        var tenantId = Guid.NewGuid();
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.UseTenant(tenantId);
        fixture.CurrentUser.IsAuthenticated.Returns(true);
        fixture.Repository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<FileItem>(_ => throw new InvalidOperationException("GetAsync throws for a file outside the tenant filter."));

        var fileId = Guid.NewGuid();
        fixture.AddFile(fileId, tenantId: null, structureKey: "cms-public", isPublic: true, mimeType: null, originalName: "گزارش.pdf");

        var result = await fixture.Service.GetDownloadContentAsync(fileId, token: null);

        result.IsForbidden.ShouldBeFalse();
        result.Content.ShouldNotBeNull();
        result.Content.Content.ShouldBe(_bytes);
        result.Content.MimeType.ShouldBe("application/octet-stream");
        result.Content.FileName.ShouldBe("گزارش.pdf");
        await fixture.Repository.DidNotReceive().GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Anonymous_caller_can_download_a_public_file()
    {
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.CurrentUser.IsAuthenticated.Returns(false);
        var fileId = Guid.NewGuid();
        fixture.AddFile(fileId, tenantId: null, structureKey: "cms-public", isPublic: true, mimeType: " image/png ", originalName: "photo.png");

        var result = await fixture.Service.GetDownloadContentAsync(fileId, token: null);

        result.IsForbidden.ShouldBeFalse();
        result.Content.ShouldNotBeNull();
        result.Content.MimeType.ShouldBe("image/png");
        result.Content.Content.ShouldBe(_bytes);
    }

    [Fact]
    public async Task Anonymous_caller_cannot_download_a_private_file_in_the_current_tenant()
    {
        var tenantId = Guid.NewGuid();
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.UseTenant(tenantId);
        fixture.CurrentUser.IsAuthenticated.Returns(false);
        var fileId = Guid.NewGuid();
        fixture.AddFile(fileId, tenantId, "private-docs", isPublic: false, mimeType: "application/pdf", originalName: "secret.pdf");

        var result = await fixture.Service.GetDownloadContentAsync(fileId, token: null);

        result.IsForbidden.ShouldBeTrue();
        result.Content.ShouldBeNull();
        await fixture.BlobContainer.DidNotReceive().GetOrNullAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Authenticated_caller_can_download_a_private_file_in_the_current_tenant()
    {
        var tenantId = Guid.NewGuid();
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.UseTenant(tenantId);
        fixture.CurrentUser.IsAuthenticated.Returns(true);
        var fileId = Guid.NewGuid();
        fixture.AddFile(fileId, tenantId, "private-docs", isPublic: false, mimeType: "application/pdf", originalName: "notes.pdf");

        var result = await fixture.Service.GetDownloadContentAsync(fileId, token: null);

        result.IsForbidden.ShouldBeFalse();
        result.Content.ShouldNotBeNull();
        result.Content.Content.ShouldBe(_bytes);
    }

    [Fact]
    public async Task Authenticated_caller_cannot_download_a_private_file_from_another_tenant()
    {
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.UseTenant(Guid.NewGuid());
        fixture.CurrentUser.IsAuthenticated.Returns(true);
        var fileId = Guid.NewGuid();
        fixture.AddFile(fileId, Guid.NewGuid(), "private-docs", isPublic: false, mimeType: "application/pdf", originalName: "other.pdf");

        var result = await fixture.Service.GetDownloadContentAsync(fileId, token: null);

        result.IsForbidden.ShouldBeFalse();
        result.Content.ShouldBeNull();
        await fixture.BlobContainer.DidNotReceive().GetOrNullAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Missing_file_is_not_found_and_does_not_throw()
    {
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.CurrentUser.IsAuthenticated.Returns(true);
        fixture.Repository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<FileItem>(_ => throw new InvalidOperationException("GetAsync must not run for a missing file."));

        var result = await fixture.Service.GetDownloadContentAsync(Guid.NewGuid(), token: null);

        result.IsForbidden.ShouldBeFalse();
        result.Content.ShouldBeNull();
    }

    [Fact]
    public async Task Matching_token_downloads_a_private_file_for_an_anonymous_caller()
    {
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.CurrentUser.IsAuthenticated.Returns(false);
        var fileId = Guid.NewGuid();
        fixture.AddFile(fileId, Guid.NewGuid(), "private-docs", isPublic: false, mimeType: "text/plain", originalName: "note.txt");
        fixture.TokenAuthorizes("signed", fileId);

        var result = await fixture.Service.GetDownloadContentAsync(fileId, "signed");

        result.IsForbidden.ShouldBeFalse();
        result.Content.ShouldNotBeNull();
        result.Content.Content.ShouldBe(_bytes);
    }

    [Fact]
    public async Task Token_for_another_file_does_not_return_that_file()
    {
        var tenantId = Guid.NewGuid();
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.UseTenant(tenantId);
        fixture.CurrentUser.IsAuthenticated.Returns(false);
        var requestedId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        fixture.AddFile(requestedId, tenantId, "private-docs", isPublic: false, mimeType: "text/plain", originalName: "a.txt", blobName: "blob-a");
        fixture.AddFile(otherId, tenantId, "private-docs", isPublic: false, mimeType: "text/plain", originalName: "b.txt", blobName: "blob-b");
        fixture.TokenAuthorizes("signed-b", otherId);

        var result = await fixture.Service.GetDownloadContentAsync(requestedId, "signed-b");

        result.IsForbidden.ShouldBeTrue();
        result.Content.ShouldBeNull();
        await fixture.BlobContainer.DidNotReceive().GetOrNullAsync("blob-b", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Blank_mime_type_falls_back_on_stream_and_thumbnail_paths()
    {
        var fixture = new DownloadAccessFixture(_bytes);
        fixture.CurrentUser.IsAuthenticated.Returns(false);
        var fileId = Guid.NewGuid();
        var item = fixture.AddFile(fileId, null, "cms-public", isPublic: true, mimeType: "   ", originalName: "clip.mp4");
        item.ThumbnailBlobName = "thumb.webp";

        var stream = await fixture.Service.GetStreamContentAsync(fileId, token: null);
        var thumbnail = await fixture.Service.GetThumbnailContentAsync(fileId, token: null);

        stream.IsForbidden.ShouldBeFalse();
        stream.Content.ShouldNotBeNull();
        stream.Content.MimeType.ShouldBe("application/octet-stream");
        thumbnail.Content.ShouldNotBeNull();
        thumbnail.Content.MimeType.ShouldBe("image/webp");
    }

    private sealed class DownloadAccessFixture
    {
        public DownloadAccessFixture(byte[] bytes)
        {
            Repository = Substitute.For<IFileItemRepository>();
            StructureCache = Substitute.For<IStructureCache>();
            BlobContainers = Substitute.For<IStructureBlobContainerProvider>();
            Tokens = Substitute.For<IFileAccessTokenService>();
            CurrentTenant = Substitute.For<ICurrentTenant>();
            CurrentUser = Substitute.For<ICurrentUser>();
            DataFilter = Substitute.For<IDataFilter>();
            BlobContainer = Substitute.For<IBlobContainer>();

            DataFilter.Disable<IMultiTenant>().Returns(Substitute.For<IDisposable>());
            CurrentTenant.Change(Arg.Any<Guid?>()).Returns(Substitute.For<IDisposable>());
            CurrentTenant.IsAvailable.Returns(false);
            BlobContainer.GetOrNullAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(_ => new MemoryStream(bytes));
            BlobContainers.GetContainerAsync(
                    Arg.Any<string?>(),
                    Arg.Any<FileStructureStorageProvider?>(),
                    Arg.Any<CancellationToken>())
                .Returns(BlobContainer);

            Service = new FileItemBlobAccessService(
                Repository,
                StructureCache,
                BlobContainers,
                Tokens,
                Substitute.For<IS3PublicBlobUrlProvider>(),
                Substitute.For<IS3PresignedUrlProvider>(),
                Options.Create(new FileManagerOptions()),
                NullLogger<FileItemBlobAccessService>.Instance,
                CurrentTenant,
                CurrentUser,
                DataFilter);
        }

        public IFileItemRepository Repository { get; }
        public IStructureCache StructureCache { get; }
        public IStructureBlobContainerProvider BlobContainers { get; }
        public IFileAccessTokenService Tokens { get; }
        public ICurrentTenant CurrentTenant { get; }
        public ICurrentUser CurrentUser { get; }
        public IDataFilter DataFilter { get; }
        public IBlobContainer BlobContainer { get; }
        public FileItemBlobAccessService Service { get; }

        public void UseTenant(Guid tenantId)
        {
            CurrentTenant.IsAvailable.Returns(true);
            CurrentTenant.Id.Returns(tenantId);
        }

        public FileItem AddFile(
            Guid id,
            Guid? tenantId,
            string structureKey,
            bool isPublic,
            string? mimeType,
            string originalName,
            string? blobName = null)
        {
            var item = new FileItem(
                id,
                tenantId,
                "stored.bin",
                originalName,
                blobName ?? "blob-" + id.ToString("N"),
                string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType,
                3,
                FileType.Document,
                structureKey);
            if (mimeType == null || string.IsNullOrWhiteSpace(mimeType))
            {
                item.MimeType = mimeType!;
            }

            Repository.FindAsync(id, Arg.Any<CancellationToken>()).Returns(item);
            StructureCache.IsPublicAccessAsync(structureKey, Arg.Any<CancellationToken>()).Returns(isPublic);
            return item;
        }

        public void TokenAuthorizes(string token, Guid fileId)
        {
            Tokens.TryValidateToken(token, out Arg.Any<Guid>())
                .Returns(call =>
                {
                    call[1] = fileId;
                    return true;
                });
        }
    }
}
