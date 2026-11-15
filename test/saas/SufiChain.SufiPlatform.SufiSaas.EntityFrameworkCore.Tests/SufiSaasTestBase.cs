using Volo.Abp;
using Volo.Abp.Testing;

namespace SufiChain.SufiPlatform.SufiSaas;

public abstract class SufiSaasTestBase : AbpIntegratedTest<IssuedLicenseSqliteTestModule>
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }
}
