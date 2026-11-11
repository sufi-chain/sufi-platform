using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.Components;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests.Components;

public class SufiErrorNavigationTests
{
    [Fact]
    public void Requested_home_url_wins()
    {
        SufiErrorNavigation.ResolveHomeUrl("/panel/dashboard", "https://console.example/404")
            .ShouldBe("/panel/dashboard");
    }

    [Fact]
    public void Panel_404_falls_back_to_the_panel_home()
    {
        SufiErrorNavigation.ResolveHomeUrl(null, "https://console.example/404?from=/panel/missing")
            .ShouldBe(SufiErrorNavigation.PanelHome);
        SufiErrorNavigation.ResolveHomeUrl(null, "https://app.example/panel/does-not-exist")
            .ShouldBe(SufiErrorNavigation.PanelHome);
    }

    [Fact]
    public void Public_404_falls_back_to_the_site_root()
    {
        SufiErrorNavigation.ResolveHomeUrl(null, "https://www.example/404")
            .ShouldBe(SufiErrorNavigation.SiteHome);
        SufiErrorNavigation.ResolveHomeUrl(null, "https://www.example/404?from=/fa/p/missing")
            .ShouldBe(SufiErrorNavigation.SiteHome);
    }
}
