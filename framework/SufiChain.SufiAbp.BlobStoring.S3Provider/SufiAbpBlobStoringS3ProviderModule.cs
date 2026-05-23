using Volo.Abp.BlobStoring;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BlobStoring.S3Provider;

[DependsOn(typeof(AbpBlobStoringModule), typeof(AbpCachingModule))]
public class SufiAbpBlobStoringS3ProviderModule : AbpModule
{
}
