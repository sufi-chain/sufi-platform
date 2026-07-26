using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

public class S3BlobProvider : BlobProviderBase, ITransientDependency
{
    protected IS3BlobNameCalculator S3BlobNameCalculator { get; }
    protected IAmazonS3ClientFactory AmazonS3ClientFactory { get; }
    protected IBlobNormalizeNamingService BlobNormalizeNamingService { get; }
    protected ILogger<S3BlobProvider>? Logger { get; }

    public S3BlobProvider(
        IS3BlobNameCalculator s3BlobNameCalculator,
        IAmazonS3ClientFactory amazonS3ClientFactory,
        IBlobNormalizeNamingService blobNormalizeNamingService,
        ILogger<S3BlobProvider>? logger = null)
    {
        S3BlobNameCalculator = s3BlobNameCalculator;
        AmazonS3ClientFactory = amazonS3ClientFactory;
        BlobNormalizeNamingService = blobNormalizeNamingService;
        Logger = logger;
    }

    public override async Task SaveAsync(BlobProviderSaveArgs args)
    {
        var blobName = S3BlobNameCalculator.Calculate(args);
        var configuration = args.Configuration.GetS3Configuration();
        var containerName = GetContainerName(args);

        using (var amazonS3Client = await AmazonS3ClientFactory.GetAmazonS3ClientAsync(configuration))
        {
            if (!args.OverrideExisting && await BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                throw new BlobAlreadyExistsException(
                    $"Saving BLOB '{args.BlobName}' does already exist in the container '{containerName}'! Set {nameof(args.OverrideExisting)} if it should be overwritten.");
            }

            if (configuration.CreateContainerIfNotExists)
            {
                await CreateContainerIfNotExistsAsync(amazonS3Client, containerName);
            }

            // When structure IsPublicAccess is true, store with public-read ACL so the object
            // is reachable at PublicBaseUrl (or derived S3 URL) without proxying through the app.
            var putRequest = new PutObjectRequest
            {
                BucketName = containerName,
                Key = blobName,
                InputStream = args.BlobStream
            };
            if (configuration.IsPublicAccess)
            {
                putRequest.CannedACL = S3CannedACL.PublicRead;
            }
            await amazonS3Client.PutObjectAsync(putRequest);
        }
    }

    public override async Task<bool> DeleteAsync(BlobProviderDeleteArgs args)
    {
        var blobName = S3BlobNameCalculator.Calculate(args);
        var containerName = GetContainerName(args);

        using (var amazonS3Client = await AmazonS3ClientFactory.GetAmazonS3ClientAsync(args.Configuration.GetS3Configuration()))
        {
            if (!await BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                return false;
            }

            await amazonS3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = containerName,
                Key = blobName
            });

            return true;
        }
    }

    public override async Task<bool> ExistsAsync(BlobProviderExistsArgs args)
    {
        var blobName = S3BlobNameCalculator.Calculate(args);
        var containerName = GetContainerName(args);
        var configuration = args.Configuration.GetS3Configuration();

        using (var amazonS3Client = await AmazonS3ClientFactory.GetAmazonS3ClientAsync(configuration))
        {
            return await BlobExistsAsync(amazonS3Client, containerName, blobName);
        }
    }

    public override async Task<Stream?> GetOrNullAsync(BlobProviderGetArgs args)
    {
        var blobName = S3BlobNameCalculator.Calculate(args);
        var containerName = GetContainerName(args);
        var configuration = args.Configuration.GetS3Configuration();

        using (var amazonS3Client = await AmazonS3ClientFactory.GetAmazonS3ClientAsync(configuration))
        {
            if (!await BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                Logger?.LogWarning("S3 blob not found: Bucket={Bucket}, Key={Key} (input BlobName={InputBlobName})",
                    containerName, blobName, args.BlobName);
                return null;
            }

            using (var response = await amazonS3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = containerName,
                Key = blobName
            }))
            {
                // Copy to MemoryStream so the returned stream is independent of the disposed S3 client.
                // ResponseStream is tied to the HTTP connection and becomes invalid when the client is disposed.
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
        }
    }

    protected virtual async Task<bool> BlobExistsAsync(AmazonS3Client amazonS3Client, string containerName, string blobName)
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(amazonS3Client, containerName))
        {
            return false;
        }

        try
        {
            await amazonS3Client.GetObjectMetadataAsync(containerName, blobName);
        }
        catch (Exception ex)
        {
            if (ex is AmazonS3Exception)
            {
                return false;
            }

            throw;
        }

        return true;
    }

    protected virtual async Task CreateContainerIfNotExistsAsync(AmazonS3Client amazonS3Client, string containerName)
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(amazonS3Client, containerName))
        {
            await amazonS3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = containerName
            });
        }
    }

    protected virtual string GetContainerName(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetS3Configuration();
        return string.IsNullOrWhiteSpace(configuration.ContainerName)
            ? args.ContainerName
            : BlobNormalizeNamingService.NormalizeContainerName(args.Configuration, configuration.ContainerName!);
    }
}
