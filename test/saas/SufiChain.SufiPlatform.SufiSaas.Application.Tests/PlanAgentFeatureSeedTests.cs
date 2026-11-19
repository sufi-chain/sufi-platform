using SufiChain.SufiPlatform.SufiAI.Hooshvare.Features;
using SufiChain.SufiPlatform.SufiSaas.Data;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

/// <summary>
/// Paid editions do not receive Agents. A host admin enables it per tenant until #127.
/// </summary>
public class PlanAgentFeatureSeedTests
{
    [Fact]
    public void No_plan_seed_turns_agents_on()
    {
        var plans = new PlanFeatures();

        foreach (var features in plans.All())
        {
            features.ContainsKey(SufiAiHooshvaresFeatures.Agents).ShouldBeFalse();
            if (features.TryGetValue(SufiAiHooshvaresFeatures.Agents, out var value))
            {
                value.ShouldBe("false");
            }
        }
    }

    private sealed class PlanFeatures : SufiSaasPlanDataSeedContributor
    {
        public PlanFeatures()
            : base(null!, null!, null!, null!, null!, null!, null!, null!, null!)
        {
        }

        public IEnumerable<Dictionary<string, string>> All()
        {
            yield return GetDemoFeatures();
            yield return GetStarterFeatures();
            yield return GetGrowthFeatures();
            yield return GetScaleFeatures();
        }
    }
}
