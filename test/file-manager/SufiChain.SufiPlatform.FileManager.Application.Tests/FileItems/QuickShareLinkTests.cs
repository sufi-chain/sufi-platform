using System;
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
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileItems;

/// <summary>
/// sufi-chain#436: Quick Share must hand out a signed, expiring <c>/s/{token}</c> link and never the raw
/// <c>file-items/{id}/download</c> URL. These tests exercise the real <see cref="FileShareLinkService"/>
/// (create, resolve, expire, revoke, tenant) rather than the static token helpers alone.
/// </summary>
public class QuickShareLinkTests
{
    private const string Secret = "quick-share-link-tests-secret";
    private const string BaseUrl = "https://stage.sufi.test/";
    private static readonly byte[] Bytes = [9, 8, 7];

    [Fact]
    public void Created_link_is_an_ascii_s_token_and_never_the_raw_download_url()
    {
        var fixture = new ShareFixture();
        var item = fixture.AddFile(Guid.NewGuid());

        var url = fixture.Service.CreateUrl(item, 60);

        // The link the share dialog shows must be the tokenized /s/ link, not the anonymous raw download URL.
        url.ShouldStartWith(BaseUrl + "s/");
        url.ShouldNotContain("/download");
        url.ShouldNotContain("/api/file-manager");

        // Latin digits only: no Persian/Arabic-Indic numerals leak into the link.
        foreach (var ch in url)
        {
            ((int)ch).ShouldBeLessThanOrEqualTo(127);
        }

        // The token is a signed "share|..." payload, so it is not a guessable bare id.
        var payload = System.Text.Encoding.UTF8.GetString(FromBase64Url(TokenOf(url).Split('.')[0]));
        payload.ShouldStartWith("share|");
        payload.ShouldContain(item.Id.ToString("N"));
    }

    [Fact]
    public async Task A_valid_unexpired_token_downloads_the_file()
    {
        var fileId = Guid.NewGuid();
        var fixture = new ShareFixture();
        var item = fixture.AddFile(fileId);
        var token = TokenOf(fixture.Service.CreateUrl(item, 60));

        var shared = await fixture.Service.ResolveAsync(token);

        shared.Status.ShouldBe(FileShareLinkStatus.Valid);
        shared.Content.ShouldNotBeNull();
        shared.Content!.Content.ShouldBe(Bytes);
    }

    [Fact]
    public async Task A_token_stops_downloading_once_it_is_past_its_expiry()
    {
        var fixture = new ShareFixture();
        var fileId = Guid.NewGuid();
        fixture.AddFile(fileId);

        var expired = FileShareTokens.Create(Secret, new FileShareTokens.Payload(
            fileId, null, DateTimeOffset.UtcNow.AddMinutes(-5), 0));

        var shared = await fixture.Service.ResolveAsync(expired);

        shared.Status.ShouldBe(FileShareLinkStatus.Expired);
        shared.Content.ShouldBeNull();
        await fixture.BlobAccess.DidNotReceive().GetContentForIntegrationAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Revoking_bumps_the_version_and_stops_every_issued_token()
    {
        var fileId = Guid.NewGuid();
        var fixture = new ShareFixture();
        var item = fixture.AddFile(fileId);
        var token = TokenOf(fixture.Service.CreateUrl(item, 60));

        // A token works before revoke.
        (await fixture.Service.ResolveAsync(token)).Status.ShouldBe(FileShareLinkStatus.Valid);

        fixture.Repository.GetAsync(fileId, Arg.Any<CancellationToken>()).Returns(item);
        await fixture.Service.RevokeAsync(fileId);

        item.GetProperty<int?>(FileShareTokens.VersionPropertyName).ShouldBe(1);

        (await fixture.Service.ResolveAsync(token)).Status.ShouldBe(FileShareLinkStatus.Revoked);
    }

    [Fact]
    public async Task A_token_minted_for_another_tenant_does_not_open_the_file()
    {
        var fixture = new ShareFixture();
        var fileId = Guid.NewGuid();
        fixture.AddFile(fileId, tenantId: Guid.NewGuid());

        var foreign = FileShareTokens.Create(Secret, new FileShareTokens.Payload(
            fileId, Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(60), 0));

        var shared = await fixture.Service.ResolveAsync(foreign);

        shared.Status.ShouldBe(FileShareLinkStatus.Invalid);
        shared.Content.ShouldBeNull();
    }

    [Fact]
    public async Task An_unknown_token_is_invalid_and_reads_no_blob()
    {
        var fixture = new ShareFixture();

        var shared = await fixture.Service.ResolveAsync("not-a-share-token");

        shared.Status.ShouldBe(FileShareLinkStatus.Invalid);
        shared.Content.ShouldBeNull();
        await fixture.Repository.DidNotReceive().FindAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static string TokenOf(string url)
    {
        var marker = url.IndexOf("/s/", StringComparison.Ordinal);
        marker.ShouldBeGreaterThanOrEqualTo(0);
        return url[(marker + 3)..];
    }

    private static byte[] FromBase64Url(string value)
    {
        var s = value.Replace('-', '+').Replace('_', '/');
        s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
        return Convert.FromBase64String(s);
    }

    private sealed class ShareFixture
    {
        public ShareFixture()
        {
            Repository = Substitute.For<IFileItemRepository>();
            DataFilter = Substitute.For<IDataFilter>();
            CurrentTenant = Substitute.For<ICurrentTenant>();

            BlobAccess = Substitute.For<FileItemBlobAccessService>(
                Repository,
                Substitute.For<IStructureCache>(),
                Substitute.For<IStructureBlobContainerProvider>(),
                Substitute.For<IFileAccessTokenService>(),
                Substitute.For<IS3PublicBlobUrlProvider>(),
                Substitute.For<IS3PresignedUrlProvider>(),
                Options.Create(new FileManagerOptions()),
                NullLogger<FileItemBlobAccessService>.Instance,
                CurrentTenant,
                Substitute.For<ICurrentUser>(),
                DataFilter);

            DataFilter.Disable<IMultiTenant>().Returns(Substitute.For<IDisposable>());
            CurrentTenant.Change(Arg.Any<Guid?>()).Returns(Substitute.For<IDisposable>());
            BlobAccess.GetContentForIntegrationAsync(Arg.Any<Guid>())
                .Returns(new FileContentResultDto
                {
                    Content = new FileContentDto
                    {
                        Content = Bytes,
                        FileName = "quick-share.txt",
                        MimeType = "text/plain"
                    }
                });

            Service = new FileShareLinkService(
                Repository,
                BlobAccess,
                DataFilter,
                CurrentTenant,
                Substitute.For<IClock>(),
                Options.Create(new FileManagerOptions
                {
                    BaseUrl = BaseUrl,
                    FileAccessTokenSecret = Secret
                }),
                Options.Create(new AbpStringEncryptionOptions()));
        }

        public IFileItemRepository Repository { get; }

        public FileItemBlobAccessService BlobAccess { get; }

        public IDataFilter DataFilter { get; }

        public ICurrentTenant CurrentTenant { get; }

        public FileShareLinkService Service { get; }

        public FileItem AddFile(Guid id, Guid? tenantId = null)
        {
            var item = new FileItem(
                id,
                tenantId,
                "quick-share",
                "quick-share.txt",
                "blob-" + id.ToString("N"),
                "text/plain",
                Bytes.Length,
                FileType.Document,
                structureKey: null);

            Repository.FindAsync(id, Arg.Any<CancellationToken>()).Returns(item);
            return item;
        }
    }
}
