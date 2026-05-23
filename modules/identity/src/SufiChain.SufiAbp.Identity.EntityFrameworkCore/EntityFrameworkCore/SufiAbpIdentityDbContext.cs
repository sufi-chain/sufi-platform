using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.Identity.EntityFrameworkCore;

[ConnectionStringName(SufiAbpIdentityDbProperties.ConnectionStringName)]
public class SufiAbpIdentityDbContext : AbpDbContext<SufiAbpIdentityDbContext>, ISufiAbpIdentityDbContext
{
    public DbSet<IdentityUser> Users { get; set; } = null!;
    public DbSet<IdentityRole> Roles { get; set; } = null!;
    public DbSet<IdentityClaimType> ClaimTypes { get; set; } = null!;
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; } = null!;
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; } = null!;
    public DbSet<IdentityLinkUser> LinkUsers { get; set; } = null!;
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; } = null!;
    public DbSet<IdentitySession> Sessions { get; set; } = null!;

    public SufiAbpIdentityDbContext(DbContextOptions<SufiAbpIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureSufiAbpIdentity();
    }
}
