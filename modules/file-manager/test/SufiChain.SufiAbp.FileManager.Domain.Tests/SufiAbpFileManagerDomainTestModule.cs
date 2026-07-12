using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerDomainModule),
    typeof(SufiAbpFileManagerTestBaseModule)
)]
public class SufiAbpFileManagerDomainTestModule : AbpModule
{

}
