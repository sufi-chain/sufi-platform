using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.SufiPlatform.Licensing;

public class LicenceStartupTests
{
    [Fact]
    public async Task Startup_runs_the_first_confirm_when_background_workers_are_disabled()
    {
        using var application = await AbpApplicationFactory.CreateAsync<LicenceStartupProbeModule>(options =>
        {
            options.Services.Configure<AbpBackgroundWorkerOptions>(worker => worker.IsEnabled = false);
            options.Services.Configure<SufiLicensingOptions>(licensing =>
            {
                licensing.LicensePath = Path.Combine(Path.GetTempPath(), "sufi-license-tests", Guid.NewGuid().ToString("N"), "missing.json");
                licensing.ConfirmUrl = "https://licence.invalid/confirm";
            });
        });
        await application.InitializeAsync();

        application.ServiceProvider.GetRequiredService<RecordingLicenseHeartbeat>().Confirmations.ShouldBe(1);
    }
}

[DependsOn(typeof(SufiLicensingRuntimeModule))]
public class LicenceStartupProbeModule : AbpModule
{
    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<RecordingLicenseHeartbeat>();
        context.Services.Replace(ServiceDescriptor.Singleton<LicenseHeartbeat>(sp => sp.GetRequiredService<RecordingLicenseHeartbeat>()));
    }
}

public sealed class RecordingLicenseHeartbeat : LicenseHeartbeat
{
    public int Confirmations { get; private set; }

    public RecordingLicenseHeartbeat(IServiceProvider services)
        : base(services)
    {
    }

    public override Task ConfirmAsync(CancellationToken cancellationToken = default)
    {
        Confirmations++;
        return base.ConfirmAsync(cancellationToken);
    }
}
