using SufiChain.SufiAbp.Authorization.Permissions;
using SufiChain.SufiAbp.Calendar.Permissions;
using SufiChain.SufiAbp.PermissionManagement;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Calendar.Data;

public class CalendarPermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string UserRoleName = "user";

    protected IPermissionDataSeeder PermissionDataSeeder { get; }

    public CalendarPermissionDataSeedContributor(IPermissionDataSeeder permissionDataSeeder)
    {
        PermissionDataSeeder = permissionDataSeeder;
    }

    public virtual Task SeedAsync(DataSeedContext context)
    {
        return PermissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            UserRoleName,
            new[]
            {
                CalendarPermissions.Calendars.Default,
                CalendarPermissions.Events.Default,
                CalendarPermissions.Events.Create,
                CalendarPermissions.Events.Update
            },
            context.TenantId);
    }
}
