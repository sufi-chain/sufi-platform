using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Xunit;

namespace SufiChain.SufiPlatform.Licensing;

public class SufiLicensingRuntimeModuleTests
{
    [Fact]
    public async Task Runtime_module_replaces_the_gate_with_the_fail_closed_LicenseGate()
    {
        using var application = await AbpApplicationFactory.CreateAsync<SufiLicensingRuntimeModule>(options =>
        {
            options.Services.Configure<AbpBackgroundWorkerOptions>(worker => worker.IsEnabled = false);
            options.Services.Configure<SufiLicensingOptions>(licensing =>
                licensing.LicensePath = Path.Combine(Path.GetTempPath(), "sufi-license-tests", Guid.NewGuid().ToString("N"), "missing.json"));
        });
        await application.InitializeAsync();

        var gate = application.ServiceProvider.GetRequiredService<ILicenseGate>();

        gate.ShouldBeOfType<LicenseGate>();
        gate.ShouldBeSameAs(application.ServiceProvider.GetRequiredService<LicenseGate>());
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();
    }
}
