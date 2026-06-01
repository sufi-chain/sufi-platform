using SufiChain.SufiAbp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatApplicationTestModule),
    typeof(ChatEntityFrameworkCoreModule),
    typeof(SufiAbpEntityFrameworkCoreSqliteModule)
)]
public class ChatEntityFrameworkCoreTestModule : AbpModule
{
}
