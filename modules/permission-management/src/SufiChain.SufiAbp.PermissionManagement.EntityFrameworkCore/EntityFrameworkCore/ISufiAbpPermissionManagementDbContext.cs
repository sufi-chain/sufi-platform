using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;

[ConnectionStringName(SufiAbpPermissionManagementDbProperties.ConnectionStringName)]
public interface ISufiAbpPermissionManagementDbContext : IEfCoreDbContext
{
    DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; }

    DbSet<PermissionDefinitionRecord> Permissions { get; }

    DbSet<PermissionGrant> PermissionGrants { get; }

    DbSet<ResourcePermissionGrant> ResourcePermissionGrants { get; }
}
