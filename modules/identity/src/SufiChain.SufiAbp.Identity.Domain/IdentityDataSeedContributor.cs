using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Identity;

public class IdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string AdminEmailPropertyName = "AdminEmail";
    public const string AdminEmailDefaultValue = "admin@sabp.com";
    public const string AdminUserNamePropertyName = "AdminUserName";
    public const string AdminUserNameDefaultValue = "admin";
    public const string AdminPasswordPropertyName = "AdminPassword";
    public const string AdminPasswordDefaultValue = "1q2w3E*";

    protected IIdentityDataSeeder IdentityDataSeeder { get; }

    public IdentityDataSeedContributor(IIdentityDataSeeder identityDataSeeder)
    {
        IdentityDataSeeder = identityDataSeeder;
    }

    public virtual Task SeedAsync(DataSeedContext context)
    {
        return IdentityDataSeeder.SeedAsync(
            context?[AdminEmailPropertyName] as string ?? AdminEmailDefaultValue,
            context?[AdminPasswordPropertyName] as string ?? AdminPasswordDefaultValue,
            context?.TenantId,
            context?[AdminUserNamePropertyName] as string ?? AdminUserNameDefaultValue
        );
    }
}
