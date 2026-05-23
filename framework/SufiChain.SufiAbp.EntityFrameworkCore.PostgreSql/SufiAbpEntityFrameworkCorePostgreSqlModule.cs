using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore.PostgreSql;

namespace SufiChain.SufiAbp.EntityFrameworkCore.PostgreSql;

[DependsOn(typeof(AbpEntityFrameworkCorePostgreSqlModule))]
public class SufiAbpEntityFrameworkCorePostgreSqlModule : AbpModule
{
}
