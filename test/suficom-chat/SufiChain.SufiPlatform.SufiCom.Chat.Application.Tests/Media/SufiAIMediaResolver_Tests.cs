using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat;

public class SufiAIMediaResolver_Tests
{
    private readonly IFileStorageIntegrationService _storage =
        Substitute.For<IFileStorageIntegrationService>();

    [Fact]
    public async Task Should_resolve_authorized_image_bytes()
    {
        var fileId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var bytes = new byte[] { 1, 2, 3 };

        _storage.GetAsync(fileId).Returns(new FileReferenceDto
        {
            Id = fileId,
            TenantId = tenantId,
            MimeType = "image/png",
            FileName = "photo.png",
            SizeInBytes = bytes.Length,
            EntityType = "Chat.Session",
            EntityId = sessionId
        });
        _storage.GetContentAsync(fileId).Returns(new FileContentBytesDto
        {
            Id = fileId,
            MimeType = "image/png",
            Content = bytes
        });

        var result = await new SufiAIMediaResolver(_storage).ResolveAsync(
            new[] { fileId },
            new SufiAIMediaResolutionContext
            {
                TenantId = tenantId,
                SessionId = sessionId
            });

        result.Count.ShouldBe(1);
        result[0].Kind.ShouldBe(SufiAIMediaKind.Image);
        result[0].Bytes.ShouldBe(bytes);
        result[0].MimeType.ShouldBe("image/png");
    }

    [Fact]
    public async Task Should_reject_cross_tenant_file_before_reading_content()
    {
        var fileId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _storage.GetAsync(fileId).Returns(new FileReferenceDto
        {
            Id = fileId,
            TenantId = Guid.NewGuid(),
            MimeType = "image/png",
            EntityType = "Chat.Session",
            EntityId = sessionId
        });

        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
            new SufiAIMediaResolver(_storage).ResolveAsync(
                new[] { fileId },
                new SufiAIMediaResolutionContext
                {
                    TenantId = Guid.NewGuid(),
                    SessionId = sessionId
                }));

        exception.Code.ShouldBe(SufiAIMediaErrorCodes.Unauthorized);
        await _storage.DidNotReceive().GetContentAsync(fileId);
    }

    [Fact]
    public async Task Should_reject_file_owned_by_another_session()
    {
        var fileId = Guid.NewGuid();

        _storage.GetAsync(fileId).Returns(new FileReferenceDto
        {
            Id = fileId,
            TenantId = Guid.NewGuid(),
            MimeType = "audio/webm",
            EntityType = "Chat.Session",
            EntityId = Guid.NewGuid()
        });

        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
            new SufiAIMediaResolver(_storage).ResolveAsync(
                new[] { fileId },
                new SufiAIMediaResolutionContext
                {
                    TenantId = Guid.NewGuid(),
                    SessionId = Guid.NewGuid()
                }));

        exception.Code.ShouldBe(SufiAIMediaErrorCodes.Unauthorized);
    }

    [Fact]
    public async Task Should_reject_unsupported_mime_without_reading_content()
    {
        var fileId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _storage.GetAsync(fileId).Returns(new FileReferenceDto
        {
            Id = fileId,
            TenantId = null,
            MimeType = "application/pdf",
            EntityType = "Chat.Session",
            EntityId = sessionId
        });

        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
            new SufiAIMediaResolver(_storage).ResolveAsync(
                new[] { fileId },
                new SufiAIMediaResolutionContext { SessionId = sessionId }));

        exception.Code.ShouldBe(SufiAIMediaErrorCodes.Unsupported);
        await _storage.DidNotReceive().GetContentAsync(fileId);
    }
}
