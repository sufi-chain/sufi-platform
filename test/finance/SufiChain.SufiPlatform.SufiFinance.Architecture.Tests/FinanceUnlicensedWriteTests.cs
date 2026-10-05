using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Licensing;
using SufiChain.SufiPlatform.SufiFinance.Accounting;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Architecture;

public class FinanceUnlicensedWriteTests
{
    [Fact]
    public async Task Finance_write_is_refused_when_no_licence_is_present()
    {
        var repository = Substitute.For<IChartOfAccountsRepository>();
        using var application = await AbpApplicationFactory.CreateAsync<FinanceUnlicensedWriteModule>(options =>
        {
            options.UseAutofac();
            options.Services.Configure<AbpBackgroundWorkerOptions>(worker => worker.IsEnabled = false);
            options.Services.Configure<SufiLicensingOptions>(licensing =>
                licensing.LicensePath = Path.Combine(Path.GetTempPath(), "sufi-license-tests", Guid.NewGuid().ToString("N"), "missing.json"));
            options.Services.Replace(ServiceDescriptor.Transient(_ => repository));
        });
        await application.InitializeAsync();

        var charts = application.ServiceProvider.GetRequiredService<IChartOfAccountsAppService>();
        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() => charts.CreateAsync(new CreateChartOfAccountsDto
        {
            Name = "Primary",
            BaseCurrencyCode = "USD"
        }));

        exception.Code.ShouldBe(LicenseErrorCodes.WriteDenied);
        exception.Data["Product"].ShouldBe(LicenseProducts.SufiFinance);
        await repository.DidNotReceiveWithAnyArgs().InsertAsync(default!, default, default);
    }
}

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SufiFinanceApplicationModule))]
public class FinanceUnlicensedWriteModule : AbpModule
{
    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysAllowAuthorization();
    }
}
