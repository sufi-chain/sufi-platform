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
        var provider = Substitute.For<ISettingProvider>();
        provider.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        provider.GetOrNullAsync(BrandingSettings.PanelLayout).Returns("sidemenu");
        var service = new TenantBrandingAppService(provider, Substitute.For<SufiSettingManager>(), Substitute.For<ICurrentTenant>());

        var branding = await service.GetAsync();

        branding.PanelLayout.ShouldBe("SideMenu");
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
