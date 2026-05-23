using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;

[ConnectionStringName(SufiAbpPermissionManagementDbProperties.ConnectionStringName)]
public class PermissionManagementDbContext : AbpDbContext<PermissionManagementDbContext>, ISufiAbpPermissionManagementDbContext
{
    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; }
    public DbSet<PermissionDefinitionRecord> Permissions { get; set; }
    public DbSet<PermissionGrant> PermissionGrants { get; set; }
    public DbSet<ResourcePermissionGrant> ResourcePermissionGrants { get; set; }

    public PermissionManagementDbContext(DbContextOptions<PermissionManagementDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSufiAbpPermissionManagement();
    }
}
