using SufiChain.SufiPlatform.Calendar.Features;
using Shouldly;
using Volo.Abp.Features;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Features;

public class CalendarAssistantFeatureTests
{
    [Fact]
    public void Assistant_feature_defaults_on()
    {
        var context = new FeatureDefinitionContext();
        new CalendarFeatureDefinitionProvider().Define(context);

        var group = context.GetGroupOrNull(CalendarFeatures.GroupName);
        group.ShouldNotBeNull();
        var feature = group!.GetFeaturesWithChildren()
            .Single(item => item.Name == CalendarFeatures.Assistant);
        feature!.DefaultValue.ShouldBe("true");
    }

    [Fact]
    public void Feature_off_hides_chrome_and_does_not_rewrite_routes()
    {
        CalendarAssistantChrome.ShowsChrome(false).ShouldBeFalse();
        CalendarAssistantChrome.RewritesPortalRoute(false).ShouldBeFalse();
    }

    [Fact]
    public void Feature_on_keeps_the_portal_rewrite_until_the_canonical_route_lands()
    {
        CalendarAssistantChrome.ShowsChrome(true).ShouldBeTrue();
        CalendarAssistantChrome.RewritesPortalRoute(true).ShouldBeTrue();
    }
}
