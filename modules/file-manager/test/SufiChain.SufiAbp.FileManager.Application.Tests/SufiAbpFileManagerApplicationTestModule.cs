using SufiChain.SufiAbp.Core;
using SufiChain.SufiAbp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerApplicationModule),
    typeof(SufiAbpFileManagerDomainTestModule)
    )]
public class SufiAbpFileManagerApplicationTestModule : SufiAbpModule
{

}
