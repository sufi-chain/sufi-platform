using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiFileManagerDomainModule),
    typeof(SufiFileManagerTestBaseModule)
)]
public class SufiFileManagerDomainTestModule : AbpModule
{

}