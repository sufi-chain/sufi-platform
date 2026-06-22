using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AI.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.Localization;

namespace SufiChain.SufiAbp.AI.Features;

/// <summary>
/// Defines AI Management edition features.
/// </summary>
public class AIFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiAIFeatures.GroupName, L("Menu:AI"));

        AddToggle(group, SufiAIFeatures.Enable);
        AddToggle(group, SufiAIFeatures.Workspaces);
        AddToggle(group, SufiAIFeatures.Chat);
        AddToggle(group, SufiAIFeatures.Audio);
        AddToggle(group, SufiAIFeatures.Vision);
        AddToggle(group, SufiAIFeatures.Embeddings);
        AddToggle(group, SufiAIFeatures.RAG);
        AddToggle(group, SufiAIFeatures.MCP);
        AddToggle(group, SufiAIFeatures.UsageAnalytics);
        AddToggle(group, SufiAIFeatures.FileManagerIntegration);
    }

    private static void AddToggle(FeatureGroupDefinition group, string name)
    {
        group.AddFeature(
            name,
            defaultValue: "true",
            displayName: L($"Feature:{name}"),
            description: L($"Feature:{name}.Description"),
            valueType: new ToggleStringValueType());
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AIResource>(name);
    }
}
