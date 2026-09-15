using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore.Sqlite;
namespace SufiChain.SufiPlatform.SufiCom.Chat;

[DependsOn(
    typeof(SufiComChatApplicationTestModule),
    typeof(SufiComChatEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class SufiComChatEntityFrameworkCoreTestModule : AbpModule
{
}
