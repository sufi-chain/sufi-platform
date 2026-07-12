using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Permissions.EntityFrameworkCore;

[ConnectionStringName(SufiPermissionsDbProperties.ConnectionStringName)]
public class PermissionsDbContext : AbpDbContext<PermissionsDbContext>, ISufiPermissionsDbContext
{
    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; }
    public DbSet<PermissionDefinitionRecord> Permissions { get; set; }
    public DbSet<PermissionGrant> PermissionGrants { get; set; }
    public DbSet<ResourcePermissionGrant> ResourcePermissionGrants { get; set; }

    public PermissionsDbContext(DbContextOptions<PermissionsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSufiPermissions();
    }
}
