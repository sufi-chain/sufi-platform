using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

/// <summary>
/// Builds direct public URLs for S3 blobs when IsPublicAccess is configured.
/// Prefers PublicBaseUrl; otherwise derives from endpoint/bucket/region.
/// </summary>
public class S3PublicBlobUrlProvider : IS3PublicBlobUrlProvider, ITransientDependency
{
    protected IBlobContainerConfigurationProvider ConfigurationProvider { get; }

    public S3PublicBlobUrlProvider(IBlobContainerConfigurationProvider configurationProvider)
    {
        ConfigurationProvider = configurationProvider;
    }

    public bool TryGetPublicBaseUrl(string containerName, out string? baseUrl)
    {
        baseUrl = null;
        var configuration = ConfigurationProvider.Get(containerName);
        if (configuration.ProviderType != typeof(S3BlobProvider))
        {
            return false;
        }

        var s3Config = configuration.GetS3Configuration();
        if (!s3Config.IsPublicAccess)
        {
            return false;
        }

        baseUrl = S3PublicUrlBuilder.ResolvePublicBaseUrl(
            s3Config.PublicBaseUrl,
            s3Config.Endpoint,
            s3Config.Region,
            s3Config.ContainerName ?? containerName,
            isPublicAccess: true);

        return !string.IsNullOrWhiteSpace(baseUrl);
    }

    public bool TryGetPublicUrl(string containerName, string blobName, Guid? tenantId, out string? url)
    {
        url = null;
        if (string.IsNullOrWhiteSpace(blobName))
        {
            return false;
        }

        if (!TryGetPublicBaseUrl(containerName, out var baseUrl) || string.IsNullOrWhiteSpace(baseUrl))
        {
            return false;
        }

        url = S3PublicUrlBuilder.BuildObjectUrl(baseUrl, blobName, tenantId);
        return true;
    }
}
