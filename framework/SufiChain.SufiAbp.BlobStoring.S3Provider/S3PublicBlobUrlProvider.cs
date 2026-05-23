using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.BlobStoring.S3Provider;

/// <summary>
/// Builds direct public URLs for S3 blobs when IsPublicAccess and PublicBaseUrl are configured.
/// S3 key format matches DefaultS3BlobNameCalculator: host/{blobName} or tenants/{tenantId}/{blobName}.
/// </summary>
public class S3PublicBlobUrlProvider : IS3PublicBlobUrlProvider, ITransientDependency
{
    protected IBlobContainerConfigurationProvider ConfigurationProvider { get; }

    public S3PublicBlobUrlProvider(IBlobContainerConfigurationProvider configurationProvider)
    {
        ConfigurationProvider = configurationProvider;
    }

    public bool TryGetPublicUrl(string containerName, string blobName, Guid? tenantId, out string? url)
    {
        url = null;
        var configuration = ConfigurationProvider.Get(containerName);
        if (configuration.ProviderType != typeof(S3BlobProvider))
            return false;
        var s3Config = configuration.GetS3Configuration();
        if (!s3Config.IsPublicAccess || string.IsNullOrWhiteSpace(s3Config.PublicBaseUrl))
            return false;
        var s3Key = tenantId == null ? $"host/{blobName}" : $"tenants/{tenantId.Value:D}/{blobName}";
        url = $"{s3Config.PublicBaseUrl!.TrimEnd('/')}/{s3Key}";
        return true;
    }
}
