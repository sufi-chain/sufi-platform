using System;
using System.Linq;
using System.Reflection;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Features;
using Xunit;
using RequiresFeatureAttribute = SufiChain.SufiPlatform.Features.RequiresFeatureAttribute;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Agents;

public class AgentFeatureAttributeTests
{
    [Fact]
    public void Agent_app_services_require_the_agents_feature()
    {
        Required(typeof(AgentMemoryAppService)).ShouldContain(SufiAiHooshvaresFeatures.Agents);
        Required(typeof(AgentToolApprovalAppService)).ShouldContain(SufiAiHooshvaresFeatures.Agents);
        Required(typeof(AgentRunApprovalAppService)).ShouldContain(SufiAiHooshvaresFeatures.Agents);
        Required(typeof(ChatAgentOrchestratorAppService)).ShouldContain(SufiAiHooshvaresFeatures.Agents);
        Required(typeof(AgentMemoryAppService)).ShouldContain(SufiAiHooshvaresFeatures.Enable);
        RequiresEveryListedFeature(typeof(AgentMemoryAppService));
        RequiresEveryListedFeature(typeof(AgentToolApprovalAppService));
        RequiresEveryListedFeature(typeof(AgentRunApprovalAppService));
        RequiresEveryListedFeature(typeof(ChatAgentOrchestratorAppService));
    }

    private static string[] Required(Type service)
    {
        return service.GetCustomAttributes(inherit: true)
            .OfType<RequiresFeatureAttribute>()
            .SelectMany(attribute => attribute.Features)
            .ToArray();
    }

    /// <summary>
    /// ABP treats a multi-feature <c>RequiresFeature</c> as OR unless <c>RequiresAll</c> is set.
    /// Agents must stay closed when <c>SufiAI.Hooshvare.Enable</c> is still on.
    /// </summary>
    private static void RequiresEveryListedFeature(Type service)
    {
        var attributes = service.GetCustomAttributes(inherit: true)
            .OfType<RequiresFeatureAttribute>()
            .Where(attribute => attribute.Features.Contains(SufiAiHooshvaresFeatures.Agents))
            .ToList();

        attributes.ShouldNotBeEmpty();
        attributes.ShouldAllBe(attribute => attribute.RequiresAll);
    }
}
