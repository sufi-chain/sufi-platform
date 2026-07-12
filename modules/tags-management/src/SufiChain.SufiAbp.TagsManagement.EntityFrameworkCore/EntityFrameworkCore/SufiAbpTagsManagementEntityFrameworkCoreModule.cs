using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TagsManagement.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpTagsManagementDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAbpTagsManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<TagsManagementDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Tags.Tag, Repositories.EfCoreTagRepository>();
            options.AddRepository<Tags.TagLink, Repositories.EfCoreTagLinkRepository>();
        });
    }
}

