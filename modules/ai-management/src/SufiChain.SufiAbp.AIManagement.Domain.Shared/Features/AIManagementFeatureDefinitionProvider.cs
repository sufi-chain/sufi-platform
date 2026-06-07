using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AIManagement.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.Localization;

namespace SufiChain.SufiAbp.AIManagement.Features;

/// <summary>
/// Defines AI Management edition features.
/// </summary>
public class AIManagementFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiAbpAIFeatures.GroupName, L("Menu:AIManagement"));

        AddToggle(group, SufiAbpAIFeatures.Enable);
        AddToggle(group, SufiAbpAIFeatures.Workspaces);
        AddToggle(group, SufiAbpAIFeatures.Chat);
        AddToggle(group, SufiAbpAIFeatures.Audio);
        AddToggle(group, SufiAbpAIFeatures.Vision);
        AddToggle(group, SufiAbpAIFeatures.Embeddings);
        AddToggle(group, SufiAbpAIFeatures.RAG);
        AddToggle(group, SufiAbpAIFeatures.MCP);
        AddToggle(group, SufiAbpAIFeatures.UsageAnalytics);
        AddToggle(group, SufiAbpAIFeatures.FileManagerIntegration);
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
        return LocalizableString.Create<AIManagementResource>(name);
    }
}
