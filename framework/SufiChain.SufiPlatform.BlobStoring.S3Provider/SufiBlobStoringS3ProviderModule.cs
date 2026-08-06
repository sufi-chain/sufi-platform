using Volo.Abp.BlobStoring;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

[DependsOn(typeof(AbpBlobStoringModule), typeof(AbpCachingModule))]
public class SufiBlobStoringS3ProviderModule : AbpModule
{
}
