using SufiChain.SufiPlatform.Authorization.Permissions;
using SufiChain.SufiPlatform.Calendar.Permissions;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace MyCompanyName.MyProjectName.Calendar;

public class CalendarUserRolePermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected IIdentityRoleRepository RoleRepository { get; }
    protected ILookupNormalizer LookupNormalizer { get; }
    protected IPermissionDataSeeder PermissionDataSeeder { get; }

    public CalendarUserRolePermissionDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        ILookupNormalizer lookupNormalizer,
        IPermissionDataSeeder permissionDataSeeder)
    {
        RoleRepository = roleRepository;
        LookupNormalizer = lookupNormalizer;
        PermissionDataSeeder = permissionDataSeeder;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        var userRole = await RoleRepository.FindByNormalizedNameAsync(
            LookupNormalizer.NormalizeName(IdentityDataSeedConsts.UserRoleNameDefaultValue));

        if (userRole == null)
        {
            return;
        }

        await PermissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            IdentityDataSeedConsts.UserRoleNameDefaultValue,
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
