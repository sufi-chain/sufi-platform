using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Branding.Branding;
using SufiChain.SufiPlatform.Branding.Settings;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using SufiSettingManager = SufiChain.SufiPlatform.Settings.ISettingManager;
using Xunit;

namespace SufiChain.SufiPlatform.Branding.Branding;

public class TenantBrandingWorkflowTests
{
    [Fact]
    public async Task Get_Should_Keep_SideMenu()
    {
        var provider = SettingProvider(("sidemenu", BrandingSettings.PanelLayout));
        var service = new TenantBrandingAppService(provider, Substitute.For<SufiSettingManager>(), Substitute.For<ICurrentTenant>());

        var branding = await service.GetAsync();

        branding.PanelLayout.ShouldBe("SideMenu");
        await provider.Received(1).GetAllAsync(Arg.Is<string[]>(names => names.Length == 9 && names.Contains(BrandingSettings.PanelLayout)));
        await provider.DidNotReceive().GetOrNullAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Get_reads_the_branding_region_in_one_batch()
    {
        var provider = SettingProvider(
            ("Sufi", BrandingSettings.AppName),
            ("#112233", BrandingSettings.ThemeColor));
        var service = new TenantBrandingAppService(provider, Substitute.For<SufiSettingManager>(), Substitute.For<ICurrentTenant>());

        var branding = await service.GetAsync();

        branding.AppName.ShouldBe("Sufi");
        branding.ThemeColor.ShouldBe("#112233");
        branding.PanelLayout.ShouldBe("DualSidebar");
        await provider.Received(1).GetAllAsync(Arg.Any<string[]>());
    }

    private static ISettingProvider SettingProvider(params (string Value, string Name)[] rows)
    {
        var provider = Substitute.For<ISettingProvider>();
        provider.GetAllAsync(Arg.Any<string[]>()).Returns(call =>
        {
            var names = call.Arg<string[]>();
            return names.Select(name => new SettingValue(name, rows.FirstOrDefault(row => row.Name == name).Value)).ToList();
        });
        return provider;
    }

    [Fact]
    public async Task Update_Should_Replace_An_Unknown_Layout_With_DualSidebar()
    {
        var manager = Substitute.For<SufiSettingManager>();
        var tenant = Substitute.For<ICurrentTenant>();
        var service = new TenantBrandingAppService(Substitute.For<ISettingProvider>(), manager, tenant);

        await service.UpdateAsync(new UpdateTenantBrandingDto { PanelLayout = "not-a-layout", AppName = "Sufi" });

        await manager.Received(1).SetForTenantOrGlobalAsync(tenant.Id, BrandingSettings.PanelLayout, "DualSidebar");
        await manager.Received(1).SetForTenantOrGlobalAsync(tenant.Id, BrandingSettings.AppName, "Sufi");
    }
}
