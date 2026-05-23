using Volo.Abp.Modularity;
using Volo.Abp.BlobStoring.Minio;

namespace SufiChain.SufiAbp.BlobStoring.Minio;

[DependsOn(typeof(AbpBlobStoringMinioModule))]
public class SufiAbpBlobStoringMinioModule : AbpModule
{
}
