namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

/// <summary>
/// Builds direct public object URLs for S3-compatible storage.
/// Key layout matches <see cref="DefaultS3BlobNameCalculator"/>: host/{blobName} or tenants/{tenantId}/{blobName}.
/// </summary>
public static class S3PublicUrlBuilder
{
    /// <summary>
    /// Resolves the public base URL: configured value wins; otherwise derives from endpoint/bucket/region when public.
    /// </summary>
    public static string? ResolvePublicBaseUrl(
        string? configuredBaseUrl,
        string? endpoint,
        string? region,
        string? containerName,
        bool isPublicAccess)
    {
        if (!isPublicAccess)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            return configuredBaseUrl.TrimEnd('/');
        }

        return DerivePublicBaseUrl(endpoint, region, containerName);
    }

    /// <summary>
    /// Derives a public base URL from S3 settings.
    /// Path-style (custom endpoint): {endpoint}/{bucket}.
    /// AWS virtual-hosted: https://{bucket}.s3.{region}.amazonaws.com.
    /// </summary>
    public static string? DerivePublicBaseUrl(string? endpoint, string? region, string? containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            return $"{endpoint.TrimEnd('/')}/{containerName.Trim()}";
        }

        var resolvedRegion = string.IsNullOrWhiteSpace(region) ? "us-east-1" : region.Trim();
        return $"https://{containerName.Trim()}.s3.{resolvedRegion}.amazonaws.com";
    }

    /// <summary>
    /// Builds the S3 object key for a blob name and tenant.
    /// </summary>
    public static string BuildObjectKey(string blobName, Guid? tenantId)
    {
        return tenantId == null
            ? $"host/{blobName}"
            : $"tenants/{tenantId.Value:D}/{blobName}";
    }

    /// <summary>
    /// Builds a full public object URL from base URL + key.
    /// </summary>
    public static string BuildObjectUrl(string publicBaseUrl, string blobName, Guid? tenantId)
    {
        var key = BuildObjectKey(blobName, tenantId);
        return $"{publicBaseUrl.TrimEnd('/')}/{key}";
    }
}
