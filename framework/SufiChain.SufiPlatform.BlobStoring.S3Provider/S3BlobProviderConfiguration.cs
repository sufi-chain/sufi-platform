using System;
using Volo.Abp.BlobStoring;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

public class S3BlobProviderConfiguration
{
    /// <summary>
    /// Custom endpoint for S3-compatible services (e.g. MinIO, DigitalOcean Spaces).
    /// When set, overrides the default AWS region endpoint.
    /// </summary>
    public string? Endpoint
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.Endpoint);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.Endpoint, value);
    }

    /// <summary>
    /// When true, uses path-style addressing for bucket URLs. Required for many S3-compatible services.
    /// Default: false.
    /// </summary>
    public bool ForcePathStyle
    {
        get => _containerConfiguration.GetConfigurationOrDefault(S3BlobProviderConfigurationNames.ForcePathStyle, false);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.ForcePathStyle, value);
    }

    public string? AccessKeyId
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.AccessKeyId);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.AccessKeyId, value);
    }

    public string? SecretAccessKey
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.SecretAccessKey);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.SecretAccessKey, value);
    }

    public bool UseCredentials
    {
        get => _containerConfiguration.GetConfigurationOrDefault(S3BlobProviderConfigurationNames.UseCredentials, false);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.UseCredentials, value);
    }

    public bool UseTemporaryCredentials
    {
        get => _containerConfiguration.GetConfigurationOrDefault(S3BlobProviderConfigurationNames.UseTemporaryCredentials, false);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.UseTemporaryCredentials, value);
    }

    public bool UseTemporaryFederatedCredentials
    {
        get => _containerConfiguration.GetConfigurationOrDefault(S3BlobProviderConfigurationNames.UseTemporaryFederatedCredentials, false);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.UseTemporaryFederatedCredentials, value);
    }

    public string? ProfileName
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.ProfileName);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.ProfileName, value);
    }

    public string? ProfilesLocation
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.ProfilesLocation);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.ProfilesLocation, value);
    }

    /// <summary>
    /// Set the validity period of the temporary access credential, the unit is s, the minimum is 900, and the maximum is 129600.
    /// </summary>
    public int DurationSeconds
    {
        get => _containerConfiguration.GetConfigurationOrDefault(S3BlobProviderConfigurationNames.DurationSeconds, 0);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.DurationSeconds, value);
    }

    public string? Name
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.Name);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.Name, value);
    }

    public string? Policy
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.Policy);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.Policy, value);
    }

    public string Region
    {
        get => _containerConfiguration.GetConfiguration<string>(S3BlobProviderConfigurationNames.Region);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.Region, value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>
    /// This name may only contain lowercase letters, numbers, and hyphens, and must begin with a letter or a number.
    /// Each hyphen must be preceded and followed by a non-hyphen character.
    /// The name must also be between 3 and 63 characters long.
    /// If this parameter is not specified, the ContainerName of the <see cref="BlobProviderArgs"/> will be used.
    /// </summary>
    public string? ContainerName
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.ContainerName);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.ContainerName, value);
    }

    /// <summary>
    /// Default value: false.
    /// </summary>
    public bool CreateContainerIfNotExists
    {
        get => _containerConfiguration.GetConfigurationOrDefault(S3BlobProviderConfigurationNames.CreateContainerIfNotExists, false);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.CreateContainerIfNotExists, value);
    }

    /// <summary>
    /// When true, blobs are stored with public-read ACL and public URLs can be built via PublicBaseUrl.
    /// Typically set from FileStructure.IsPublicAccess.
    /// </summary>
    public bool IsPublicAccess
    {
        get => _containerConfiguration.GetConfigurationOrDefault(S3BlobProviderConfigurationNames.IsPublicAccess, false);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.IsPublicAccess, value);
    }

    /// <summary>
    /// Base URL for constructing direct public URLs (e.g. https://bucket.s3.region.amazonaws.com/ or CDN).
    /// When set with IsPublicAccess, GetPublicUrl returns direct S3 object URLs.
    /// Typically set from FileStructure.BaseUrl.
    /// </summary>
    public string? PublicBaseUrl
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.PublicBaseUrl);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.PublicBaseUrl, value);
    }

    private readonly string _temporaryCredentialsCacheKey;

    public string? TemporaryCredentialsCacheKey
    {
        get => _containerConfiguration.GetConfigurationOrDefault(S3BlobProviderConfigurationNames.TemporaryCredentialsCacheKey, _temporaryCredentialsCacheKey);
        set => _containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.TemporaryCredentialsCacheKey, value);
    }

    private readonly BlobContainerConfiguration _containerConfiguration;

    public S3BlobProviderConfiguration(BlobContainerConfiguration containerConfiguration)
    {
        _containerConfiguration = containerConfiguration;
        _temporaryCredentialsCacheKey = Guid.NewGuid().ToString("N");
    }
}
