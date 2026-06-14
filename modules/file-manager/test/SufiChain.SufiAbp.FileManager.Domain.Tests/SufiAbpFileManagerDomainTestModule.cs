using SufiChain.SufiAbp.Core;
using SufiChain.SufiAbp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerDomainModule),
    typeof(SufiAbpFileManagerTestBaseModule)
)]
public class SufiAbpFileManagerDomainTestModule : SufiAbpModule
{

}
