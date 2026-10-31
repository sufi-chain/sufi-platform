using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Permissions;

public class PermissionDataSeeder : IPermissionDataSeeder, ITransientDependency
{
    protected IPermissionGrantRepository PermissionGrantRepository { get; }
    protected IGuidGenerator GuidGenerator { get; }

    protected ICurrentTenant CurrentTenant { get; }

    public PermissionDataSeeder(
        IPermissionGrantRepository permissionGrantRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        PermissionGrantRepository = permissionGrantRepository;
        GuidGenerator = guidGenerator;
        CurrentTenant = currentTenant;
    }

    public virtual async Task SeedAsync(
        string providerName,
        string providerKey,
        IEnumerable<string> grantedPermissions,
        Guid? tenantId = null)
    {
        using (CurrentTenant.Change(tenantId))
        {
            using (PermissionGrantRepository.DisableTracking())
            {
                var names = grantedPermissions.Distinct(StringComparer.Ordinal).ToArray();
                var existsPermissionGrants = (await PermissionGrantRepository.GetListAsync(names, providerName, providerKey)).Select(x => x.Name).ToList();
                var permissions = names.Except(existsPermissionGrants).Select(permissionName => new PermissionGrant(GuidGenerator.Create(), permissionName, providerName, providerKey, tenantId)).ToList();
                if (!permissions.Any())
                {
                    return;
                }

                // Flush now. Another contributor in the same unit of work can grant the same
                // name, and this query does not see unsaved rows. A host grant has a null
                // tenant id, which SQL Server does not treat as a duplicate. A tenant id does.
                await PermissionGrantRepository.InsertManyAsync(permissions, autoSave: true);
            }
        }
    }
}
