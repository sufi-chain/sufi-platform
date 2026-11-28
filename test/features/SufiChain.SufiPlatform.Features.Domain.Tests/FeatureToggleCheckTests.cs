using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Features;

public class FeatureToggleCheckTests
{
    [Theory]
    [InlineData(0, 3, FeatureToggleCheckState.Off)]
    [InlineData(2, 3, FeatureToggleCheckState.Indeterminate)]
    [InlineData(3, 3, FeatureToggleCheckState.On)]
    [InlineData(0, 0, FeatureToggleCheckState.Off)]
    public void Some_enabled_toggles_are_indeterminate(int enabled, int total, FeatureToggleCheckState expected)
    {
        FeatureToggleCheck.FromCounts(enabled, total).ShouldBe(expected);
    }
}
