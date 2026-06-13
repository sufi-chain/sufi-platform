using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(FileManagerApplicationModule),
    typeof(FileManagerDomainTestModule)
    )]
public class SufiAbpFileManagerApplicationTestModule : SufiAbpModule
{

}
