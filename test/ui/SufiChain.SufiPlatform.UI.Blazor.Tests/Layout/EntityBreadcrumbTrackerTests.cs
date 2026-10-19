using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.Layout;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.Services.Layout;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests.Layout;

public class EntityBreadcrumbTrackerTests
{
    [Fact]
    public async Task Appends_entity_name_after_menu_trail_and_survives_rebuild()
    {
        var layout = new DefaultPageLayout();
        using var tracker = new EntityBreadcrumbTracker(layout);
        tracker.Set("gpt-4o");

        layout.BreadcrumbItems.Count.ShouldBe(0);

        layout.BreadcrumbItems.Add(new BreadcrumbItem("AI", "/ai"));
        layout.BreadcrumbItems.Add(new BreadcrumbItem("Model configurations", "/ai/models"));
        await WaitUntilAsync(() => layout.BreadcrumbItems.Count == 3);
        layout.BreadcrumbItems.Select(x => x.Text).ShouldBe(new[] { "AI", "Model configurations", "gpt-4o" });

        layout.Reset();
        layout.BreadcrumbItems.Add(new BreadcrumbItem("AI", "/ai"));
        await WaitUntilAsync(() => layout.BreadcrumbItems.Count == 2);
        layout.BreadcrumbItems.Select(x => x.Text).ShouldBe(new[] { "AI", "gpt-4o" });
    }

    [Fact]
    public void Renaming_replaces_the_crumb_and_does_not_duplicate()
    {
        var layout = new DefaultPageLayout();
        layout.BreadcrumbItems.Add(new BreadcrumbItem("Workspaces", "/ai/workspaces"));
        using var tracker = new EntityBreadcrumbTracker(layout);

        tracker.Set("Default");
        tracker.Set("Default");
        tracker.Set("Sufi infra");

        layout.BreadcrumbItems.Select(x => x.Text).ShouldBe(new[] { "Workspaces", "Sufi infra" });
    }

    [Fact]
    public void Skips_when_last_crumb_already_is_the_entity_and_raises_layout_change()
    {
        var layout = new DefaultPageLayout();
        var changes = 0;
        layout.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(IPageLayout.BreadcrumbItems)) changes++; };
        layout.BreadcrumbItems.Add(new BreadcrumbItem("Assistant"));
        using var tracker = new EntityBreadcrumbTracker(layout);

        tracker.Set("assistant");

        layout.BreadcrumbItems.Count.ShouldBe(1);
        changes.ShouldBeGreaterThan(0);
    }

    // Polls instead of a fixed 500 ms settle: the tracker re-appends asynchronously and a loaded CI runner can take longer.
    private static async Task WaitUntilAsync(System.Func<bool> condition, int timeoutMs = 5000)
    {
        var waited = 0;
        while (!condition() && waited < timeoutMs)
        {
            await Task.Delay(25);
            waited += 25;
        }
    }
}
