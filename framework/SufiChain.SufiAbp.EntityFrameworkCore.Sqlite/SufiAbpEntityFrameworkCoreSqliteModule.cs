using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore.Sqlite;

namespace SufiChain.SufiAbp.EntityFrameworkCore.Sqlite;

[DependsOn(typeof(AbpEntityFrameworkCoreSqliteModule))]
public class SufiAbpEntityFrameworkCoreSqliteModule : AbpModule
{
}
