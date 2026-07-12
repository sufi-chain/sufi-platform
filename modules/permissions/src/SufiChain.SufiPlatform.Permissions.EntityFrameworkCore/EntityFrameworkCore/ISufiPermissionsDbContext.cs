using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Permissions.EntityFrameworkCore;

[ConnectionStringName(SufiPermissionsDbProperties.ConnectionStringName)]
public interface ISufiPermissionsDbContext : IEfCoreDbContext
{
    DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; }

    DbSet<PermissionDefinitionRecord> Permissions { get; }

    DbSet<PermissionGrant> PermissionGrants { get; }

    DbSet<ResourcePermissionGrant> ResourcePermissionGrants { get; }
}
