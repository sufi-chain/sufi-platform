using Shouldly;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.Services.Layout;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests.Layout;

public class DefaultPageLayoutNavigationTests
{
    [Fact]
    public void Reset_keeps_content_published_for_the_open_navigation()
    {
        var layout = new DefaultPageLayout();
        layout.Title = "Tenants";
        layout.BreadcrumbItems.Add(new BreadcrumbItem("Tenants"));

        layout.BeginNavigation();
        layout.Title = "Dashboard";
        layout.BreadcrumbItems.Clear();
        layout.BreadcrumbItems.Add(new BreadcrumbItem("Dashboard"));

        layout.Reset();

        layout.Title.ShouldBe("Dashboard");
        layout.BreadcrumbItems.Select(item => item.Text).ShouldBe(new[] { "Dashboard" });
    }

    [Fact]
    public void Reset_clears_the_previous_page_when_the_next_page_sets_nothing()
    {
        var layout = new DefaultPageLayout();
        layout.Title = "Tenants";
        layout.Subtitle = "Host";
        layout.BreadcrumbItems.Add(new BreadcrumbItem("Tenants"));

        layout.BeginNavigation();
        layout.Reset();

        layout.Title.ShouldBeNull();
        layout.Subtitle.ShouldBeNull();
        layout.BreadcrumbItems.ShouldBeEmpty();
    }

    [Fact]
    public void Reset_without_a_navigation_still_clears()
    {
        var layout = new DefaultPageLayout();
        layout.Title = "Dashboard";
        layout.BreadcrumbItems.Add(new BreadcrumbItem("Dashboard"));

        layout.Reset();

        layout.Title.ShouldBeNull();
        layout.BreadcrumbItems.ShouldBeEmpty();
    }
}
