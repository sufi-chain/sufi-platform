using Volo.Abp.BlobStoring;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

public interface IS3BlobNameCalculator
{
    string Calculate(BlobProviderArgs args);
}
