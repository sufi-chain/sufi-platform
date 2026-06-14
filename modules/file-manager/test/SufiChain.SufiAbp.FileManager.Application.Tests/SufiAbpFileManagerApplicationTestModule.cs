using SufiChain.SufiAbp.Modularity;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerApplicationModule),
    typeof(SufiAbpFileManagerDomainTestModule)
    )]
public class SufiAbpFileManagerApplicationTestModule : SufiAbpModule
{

}
