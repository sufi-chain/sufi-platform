using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerApplicationModule),
    typeof(SufiAbpFileManagerDomainTestModule)
    )]
public class SufiAbpFileManagerApplicationTestModule : AbpModule
{

}
