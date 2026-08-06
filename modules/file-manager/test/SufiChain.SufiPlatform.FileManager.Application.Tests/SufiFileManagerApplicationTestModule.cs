using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiFileManagerApplicationModule),
    typeof(SufiFileManagerDomainTestModule)
    )]
public class SufiFileManagerApplicationTestModule : AbpModule
{

}