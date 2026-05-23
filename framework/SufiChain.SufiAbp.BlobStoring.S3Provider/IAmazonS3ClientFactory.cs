using System.Threading.Tasks;
using Amazon.S3;

namespace SufiChain.SufiAbp.BlobStoring.S3Provider;

public interface IAmazonS3ClientFactory
{
    Task<AmazonS3Client> GetAmazonS3ClientAsync(S3BlobProviderConfiguration configuration);
}
