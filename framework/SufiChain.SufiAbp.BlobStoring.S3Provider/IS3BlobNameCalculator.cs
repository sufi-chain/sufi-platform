using Volo.Abp.BlobStoring;

namespace SufiChain.SufiAbp.BlobStoring.S3Provider;

public interface IS3BlobNameCalculator
{
    string Calculate(BlobProviderArgs args);
}
