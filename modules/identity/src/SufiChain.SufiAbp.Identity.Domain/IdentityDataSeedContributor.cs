using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Identity;

public class IdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string AdminEmailPropertyName = "AdminEmail";
    public const string AdminUserNamePropertyName = "AdminUserName";
    public const string AdminPasswordPropertyName = "AdminPassword";

    protected IIdentityDataSeeder IdentityDataSeeder { get; }

    public IdentityDataSeedContributor(IIdentityDataSeeder identityDataSeeder)
    {
        IdentityDataSeeder = identityDataSeeder;
    }

    public virtual Task SeedAsync(DataSeedContext context)
    {
        return IdentityDataSeeder.SeedAsync(
            context?[AdminEmailPropertyName] as string ?? IdentityDataSeedConsts.AdminEmailDefaultValue,
            context?[AdminPasswordPropertyName] as string ?? IdentityDataSeedConsts.AdminPasswordDefaultValue,
            context?.TenantId,
            context?[AdminUserNamePropertyName] as string ?? IdentityDataSeedConsts.AdminUserNameDefaultValue
        );
    }
}
