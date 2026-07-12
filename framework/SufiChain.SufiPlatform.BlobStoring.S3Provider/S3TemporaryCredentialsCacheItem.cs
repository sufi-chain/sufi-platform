using System;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

[CacheName("S3TemporaryCredentials")]
[Serializable]
public class S3TemporaryCredentialsCacheItem
{
    public string AccessKeyId { get; set; } = default!;

    public string SecretAccessKey { get; set; } = default!;

    public string SessionToken { get; set; } = default!;

    public S3TemporaryCredentialsCacheItem()
    {
    }

    public S3TemporaryCredentialsCacheItem(string accessKeyId, string secretAccessKey, string sessionToken)
    {
        AccessKeyId = accessKeyId;
        SecretAccessKey = secretAccessKey;
        SessionToken = sessionToken;
    }
}
