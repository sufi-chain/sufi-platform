using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

public enum SufiAIMediaKind { Image, Audio, Unsupported }

public sealed class SufiAIMedia
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    public SufiAIMediaKind Kind { get; set; }
}

public sealed class SufiAIMediaResolutionContext
{
    public Guid? TenantId { get; set; }
    public Guid? SessionId { get; set; }
    public bool AllowOperatorGallery { get; set; }
}

public static class SufiAIMediaErrorCodes
{
    public const string StorageFailure = "SufiAI:MediaStorageFailure";
    public const string Unauthorized = "SufiAI:MediaUnauthorized";
    public const string Unsupported = "SufiAI:UnsupportedMedia";
    public const string TooLarge = "SufiAI:MediaTooLarge";
    public const string TranscriptionFailed = "SufiAI:TranscriptionFailed";
    public const string ProviderUnavailable = "SufiAI:ProviderUnavailable";
}

public interface ISufiAIMediaResolver
{
    Task<IReadOnlyList<SufiAIMedia>> ResolveAsync(
        IEnumerable<Guid> fileIds,
        SufiAIMediaResolutionContext? context = null,
        CancellationToken cancellationToken = default);
}

public sealed class SufiAIMediaResolver : ISufiAIMediaResolver, ITransientDependency
{
    private const long MaxBytes = 10 * 1024 * 1024;
    private readonly IFileStorageIntegrationService _storage;

    public SufiAIMediaResolver(IFileStorageIntegrationService storage) => _storage = storage;

    public async Task<IReadOnlyList<SufiAIMedia>> ResolveAsync(
        IEnumerable<Guid> fileIds,
        SufiAIMediaResolutionContext? context = null,
        CancellationToken cancellationToken = default)
    {
        context ??= new SufiAIMediaResolutionContext();
        var result = new List<SufiAIMedia>();
        foreach (var id in fileIds.Where(x => x != Guid.Empty).Distinct())
        {
            FileReferenceDto metadata;
            try
            {
                metadata = await _storage.GetAsync(id);
            }
            catch (Exception ex)
            {
                throw new BusinessException(SufiAIMediaErrorCodes.StorageFailure, innerException: ex);
            }

            if (metadata.Id != id)
                throw new BusinessException(SufiAIMediaErrorCodes.StorageFailure);

            if (context.TenantId.HasValue && metadata.TenantId != context.TenantId)
                throw new BusinessException(SufiAIMediaErrorCodes.Unauthorized);

            if (!context.AllowOperatorGallery &&
                (context.SessionId is null ||
                 !string.Equals(metadata.EntityType, "Chat.Session", StringComparison.OrdinalIgnoreCase) ||
                 metadata.EntityId != context.SessionId))
                throw new BusinessException(SufiAIMediaErrorCodes.Unauthorized);

            if (metadata.SizeInBytes > MaxBytes)
                throw new BusinessException(SufiAIMediaErrorCodes.TooLarge);

            var kind = metadata.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                ? SufiAIMediaKind.Image
                : metadata.MimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
                    ? SufiAIMediaKind.Audio
                    : SufiAIMediaKind.Unsupported;
            if (kind == SufiAIMediaKind.Unsupported)
                throw new BusinessException(SufiAIMediaErrorCodes.Unsupported);

            FileContentBytesDto content;
            try
            {
                content = await _storage.GetContentAsync(id);
            }
            catch (Exception ex)
            {
                throw new BusinessException(SufiAIMediaErrorCodes.StorageFailure, innerException: ex);
            }

            if (content.Id != id)
                throw new BusinessException(SufiAIMediaErrorCodes.StorageFailure);
            if (content.Content.Length == 0 || content.Content.LongLength > MaxBytes)
                throw new BusinessException(
                    content.Content.LongLength > MaxBytes
                        ? SufiAIMediaErrorCodes.TooLarge
                        : SufiAIMediaErrorCodes.StorageFailure);

            var mimeType = string.IsNullOrWhiteSpace(content.MimeType) ? metadata.MimeType : content.MimeType;
            if (!string.Equals(mimeType, metadata.MimeType, StringComparison.OrdinalIgnoreCase))
                throw new BusinessException(SufiAIMediaErrorCodes.Unsupported);

            result.Add(new SufiAIMedia
            {
                FileId = id,
                FileName = metadata.FileName,
                MimeType = mimeType,
                Bytes = content.Content,
                Kind = kind
            });
        }
        return result;
    }
}
