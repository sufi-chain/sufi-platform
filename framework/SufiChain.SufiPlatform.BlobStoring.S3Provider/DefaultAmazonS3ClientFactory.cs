using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

public class DefaultAmazonS3ClientFactory : IAmazonS3ClientFactory, ITransientDependency
{
    protected IDistributedCache<S3TemporaryCredentialsCacheItem> Cache { get; }

    protected IStringEncryptionService StringEncryptionService { get; }

    public DefaultAmazonS3ClientFactory(
        IDistributedCache<S3TemporaryCredentialsCacheItem> cache,
        IStringEncryptionService stringEncryptionService)
    {
        Cache = cache;
        StringEncryptionService = stringEncryptionService;
    }

    public virtual async Task<AmazonS3Client> GetAmazonS3ClientAsync(S3BlobProviderConfiguration configuration)
    {
        var s3Config = CreateS3Config(configuration);

        if (configuration.UseCredentials)
        {
            var awsCredentials = GetAwsCredentials(configuration);
            return awsCredentials == null
                ? new AmazonS3Client(s3Config)
                : new AmazonS3Client(awsCredentials, s3Config);
        }

        if (configuration.UseTemporaryCredentials)
        {
            return new AmazonS3Client(await GetTemporaryCredentialsAsync(configuration), s3Config);
        }

        if (configuration.UseTemporaryFederatedCredentials)
        {
            return new AmazonS3Client(await GetTemporaryFederatedCredentialsAsync(configuration), s3Config);
        }

        if (string.IsNullOrWhiteSpace(configuration.AccessKeyId))
        {
            throw new ArgumentNullException(nameof(configuration.AccessKeyId), "AccessKeyId is required when not using credentials or temporary credentials.");
        }

        if (string.IsNullOrWhiteSpace(configuration.SecretAccessKey))
        {
            throw new ArgumentNullException(nameof(configuration.SecretAccessKey), "SecretAccessKey is required when not using credentials or temporary credentials.");
        }

        return new AmazonS3Client(configuration.AccessKeyId, configuration.SecretAccessKey, s3Config);
    }

    protected virtual AmazonS3Config CreateS3Config(S3BlobProviderConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.Endpoint))
        {
            var config = new AmazonS3Config
            {
                ServiceURL = configuration.Endpoint.TrimEnd('/'),
                ForcePathStyle = configuration.ForcePathStyle
            };
            return config;
        }

        var region = RegionEndpoint.GetBySystemName(configuration.Region);
        return new AmazonS3Config { RegionEndpoint = region };
    }

    protected virtual AWSCredentials? GetAwsCredentials(S3BlobProviderConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.ProfileName))
        {
            return null;
        }

        var chain = new CredentialProfileStoreChain(configuration.ProfilesLocation);

        if (chain.TryGetAWSCredentials(configuration.ProfileName, out var awsCredentials))
        {
            return awsCredentials;
        }

        throw new AmazonS3Exception("AWS credentials not found for the specified profile.");
    }

    protected virtual async Task<SessionAWSCredentials> GetTemporaryCredentialsAsync(S3BlobProviderConfiguration configuration)
    {
        var temporaryCredentialsCache = await Cache.GetAsync(configuration.TemporaryCredentialsCacheKey!);

        if (temporaryCredentialsCache == null)
        {
            AmazonSecurityTokenServiceClient stsClient;

            if (!string.IsNullOrEmpty(configuration.AccessKeyId) && !string.IsNullOrEmpty(configuration.SecretAccessKey))
            {
                stsClient = new AmazonSecurityTokenServiceClient(configuration.AccessKeyId, configuration.SecretAccessKey);
            }
            else
            {
                var awsCredentials = GetAwsCredentials(configuration);
                stsClient = awsCredentials == null
                    ? new AmazonSecurityTokenServiceClient()
                    : new AmazonSecurityTokenServiceClient(awsCredentials);
            }

            using (stsClient)
            {
                var getSessionTokenRequest = new GetSessionTokenRequest
                {
                    DurationSeconds = configuration.DurationSeconds
                };

                var sessionTokenResponse = await stsClient.GetSessionTokenAsync(getSessionTokenRequest);
                var credentials = sessionTokenResponse.Credentials;

                temporaryCredentialsCache = await SetTemporaryCredentialsCacheAsync(configuration, credentials);
            }
        }

        var sessionCredentials = new SessionAWSCredentials(
            StringEncryptionService.Decrypt(temporaryCredentialsCache.AccessKeyId),
            StringEncryptionService.Decrypt(temporaryCredentialsCache.SecretAccessKey),
            StringEncryptionService.Decrypt(temporaryCredentialsCache.SessionToken));
        return sessionCredentials;
    }

    protected virtual async Task<SessionAWSCredentials> GetTemporaryFederatedCredentialsAsync(S3BlobProviderConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            throw new ArgumentNullException(nameof(configuration.Name), "Name is required for federated credentials.");
        }

        if (string.IsNullOrWhiteSpace(configuration.Policy))
        {
            throw new ArgumentNullException(nameof(configuration.Policy), "Policy is required for federated credentials.");
        }

        var temporaryCredentialsCache = await Cache.GetAsync(configuration.TemporaryCredentialsCacheKey!);

        if (temporaryCredentialsCache == null)
        {
            AmazonSecurityTokenServiceClient stsClient;

            if (!string.IsNullOrEmpty(configuration.AccessKeyId) && !string.IsNullOrEmpty(configuration.SecretAccessKey))
            {
                stsClient = new AmazonSecurityTokenServiceClient(configuration.AccessKeyId, configuration.SecretAccessKey);
            }
            else
            {
                var awsCredentials = GetAwsCredentials(configuration);
                stsClient = awsCredentials == null
                    ? new AmazonSecurityTokenServiceClient()
                    : new AmazonSecurityTokenServiceClient(awsCredentials);
            }

            using (stsClient)
            {
                var federationTokenRequest = new GetFederationTokenRequest
                {
                    DurationSeconds = configuration.DurationSeconds,
                    Name = configuration.Name,
                    Policy = configuration.Policy
                };

                var federationTokenResponse = await stsClient.GetFederationTokenAsync(federationTokenRequest);
                var credentials = federationTokenResponse.Credentials;

                temporaryCredentialsCache = await SetTemporaryCredentialsCacheAsync(configuration, credentials);
            }
        }

        var sessionCredentials = new SessionAWSCredentials(
            StringEncryptionService.Decrypt(temporaryCredentialsCache.AccessKeyId),
            StringEncryptionService.Decrypt(temporaryCredentialsCache.SecretAccessKey),
            StringEncryptionService.Decrypt(temporaryCredentialsCache.SessionToken));
        return sessionCredentials;
    }

    private async Task<S3TemporaryCredentialsCacheItem> SetTemporaryCredentialsCacheAsync(
        S3BlobProviderConfiguration configuration,
        Credentials credentials)
    {
        var temporaryCredentialsCache = new S3TemporaryCredentialsCacheItem(
            StringEncryptionService.Encrypt(credentials.AccessKeyId)!,
            StringEncryptionService.Encrypt(credentials.SecretAccessKey)!,
            StringEncryptionService.Encrypt(credentials.SessionToken)!);

        await Cache.SetAsync(configuration.TemporaryCredentialsCacheKey!, temporaryCredentialsCache,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(configuration.DurationSeconds - 10)
            });

        return temporaryCredentialsCache;
    }
}
