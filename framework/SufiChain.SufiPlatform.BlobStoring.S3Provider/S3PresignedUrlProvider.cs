using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

/// <summary>
/// Generates presigned download URLs for S3 blobs when IsPublicAccess is false and PublicBaseUrl is configured.
/// S3 key format matches DefaultS3BlobNameCalculator: host/{blobName} or tenants/{tenantId}/{blobName}.
/// </summary>
public class S3PresignedUrlProvider : IS3PresignedUrlProvider, ITransientDependency
{
    private const int MinValidityMinutes = 1;
    private const int MaxValidityMinutes = 7 * 24 * 60; // 7 days

    protected IBlobContainerConfigurationProvider ConfigurationProvider { get; }
    protected IAmazonS3ClientFactory AmazonS3ClientFactory { get; }
    protected ILogger<S3PresignedUrlProvider>? Logger { get; }

    public S3PresignedUrlProvider(
        IBlobContainerConfigurationProvider configurationProvider,
        IAmazonS3ClientFactory amazonS3ClientFactory,
        ILogger<S3PresignedUrlProvider>? logger = null)
    {
        ConfigurationProvider = configurationProvider;
        AmazonS3ClientFactory = amazonS3ClientFactory;
        Logger = logger;
    }

    public async Task<string?> GetPresignedDownloadUrlAsync(string containerName, string blobName, Guid? tenantId, TimeSpan validity)
    {
        var configuration = ConfigurationProvider.Get(containerName);
        if (configuration.ProviderType != typeof(S3BlobProvider))
            return null;

        var s3Config = configuration.GetS3Configuration();
        if (s3Config.IsPublicAccess || string.IsNullOrWhiteSpace(s3Config.PublicBaseUrl))
            return null;

        if (string.IsNullOrWhiteSpace(blobName))
            return null;

        var validityMinutes = Math.Clamp(validity.TotalMinutes, MinValidityMinutes, MaxValidityMinutes);
        var expires = DateTime.UtcNow.AddMinutes(validityMinutes);

        var s3Key = S3PublicUrlBuilder.BuildObjectKey(blobName, tenantId);
        var bucketName = string.IsNullOrWhiteSpace(s3Config.ContainerName)
            ? containerName
            : s3Config.ContainerName;

        try
        {
            using var amazonS3Client = await AmazonS3ClientFactory.GetAmazonS3ClientAsync(s3Config);
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = s3Key,
                Expires = expires
            };
            var url = amazonS3Client.GetPreSignedURL(request);
            return url;
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Failed to generate presigned URL for container={Container}, key={Key}",
                containerName, s3Key);
            return null;
        }
    }
}
