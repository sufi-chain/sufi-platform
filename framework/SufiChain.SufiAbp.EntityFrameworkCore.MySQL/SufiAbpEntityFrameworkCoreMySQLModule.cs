using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore.MySQL;

namespace SufiChain.SufiAbp.EntityFrameworkCore.MySQL;

[DependsOn(typeof(AbpEntityFrameworkCoreMySQLModule))]
public class SufiAbpEntityFrameworkCoreMySQLModule : AbpModule
{
}
