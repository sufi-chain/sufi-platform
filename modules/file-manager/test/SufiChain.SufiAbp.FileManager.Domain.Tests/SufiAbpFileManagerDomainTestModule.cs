using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(FileManagerDomainModule),
    typeof(FileManagerTestBaseModule)
)]
public class SufiAbpFileManagerDomainTestModule : AbpModule
{

}
