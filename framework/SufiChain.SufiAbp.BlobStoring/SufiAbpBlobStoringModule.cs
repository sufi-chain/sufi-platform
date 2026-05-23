using Volo.Abp.BlobStoring;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BlobStoring;

[DependsOn(
    typeof(AbpBlobStoringModule)
)]
public class SufiAbpBlobStoringModule : AbpModule
{
}
