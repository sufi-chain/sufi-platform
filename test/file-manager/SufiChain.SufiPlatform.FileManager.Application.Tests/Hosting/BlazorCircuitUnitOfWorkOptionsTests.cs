using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.AspNetCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Uow;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Hosting;

[DependsOn(typeof(SufiAspNetCoreModule))]
public class BlazorCircuitUnitOfWorkTestModule : AbpModule
{
}

public class BlazorCircuitUnitOfWorkOptionsTests : AbpIntegratedTest<BlazorCircuitUnitOfWorkTestModule>
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    [Fact]
    public void Blazor_circuit_is_excluded_from_the_request_unit_of_work()
    {
        var options = GetRequiredService<IOptions<AbpAspNetCoreUnitOfWorkOptions>>().Value;

        options.IgnoredUrls.ShouldContain("/_blazor");
    }
}
