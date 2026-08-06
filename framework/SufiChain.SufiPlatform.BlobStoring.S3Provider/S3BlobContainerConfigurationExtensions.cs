using System;
using Volo.Abp.BlobStoring;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

public static class S3BlobContainerConfigurationExtensions
{
    public static S3BlobProviderConfiguration GetS3Configuration(this BlobContainerConfiguration containerConfiguration)
    {
        return new S3BlobProviderConfiguration(containerConfiguration);
    }

    public static BlobContainerConfiguration UseS3(
        this BlobContainerConfiguration containerConfiguration,
        Action<S3BlobProviderConfiguration> s3ConfigureAction)
    {
        containerConfiguration.ProviderType = typeof(S3BlobProvider);
        containerConfiguration.NamingNormalizers.TryAdd<S3BlobNamingNormalizer>();

        s3ConfigureAction(new S3BlobProviderConfiguration(containerConfiguration));

        return containerConfiguration;
    }
}
