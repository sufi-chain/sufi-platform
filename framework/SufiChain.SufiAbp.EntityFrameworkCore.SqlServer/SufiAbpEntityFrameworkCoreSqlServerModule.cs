using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore.SqlServer;

namespace SufiChain.SufiAbp.EntityFrameworkCore.SqlServer;

[DependsOn(typeof(AbpEntityFrameworkCoreSqlServerModule))]
public class SufiAbpEntityFrameworkCoreSqlServerModule : AbpModule
{
}
