using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Localization;
using NumericValueValidator = Volo.Abp.Validation.StringValues.NumericValueValidator;

namespace SufiChain.SufiPlatform.SufiAI.Features;

/// <summary>
/// Defines AI Management edition features.
/// </summary>
public class AIFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiAIFeatures.GroupName, L("Menu:SufiAI"));

        AddToggle(group, SufiAIFeatures.Enable);
        AddToggle(group, SufiAIFeatures.Workspaces);
        AddToggle(group, SufiAIFeatures.Chat);
        AddToggle(group, SufiAIFeatures.Audio);
        AddToggle(group, SufiAIFeatures.Vision);
        AddToggle(group, SufiAIFeatures.Embeddings);
        AddToggle(group, SufiAIFeatures.WebSearch);
        AddToggle(group, SufiAIFeatures.WebFetch);
        AddToggle(group, SufiAIFeatures.RAG);
        AddToggle(group, SufiAIFeatures.MCP);
        AddToggle(group, SufiAIFeatures.UsageAnalytics);
        AddToggle(group, SufiAIFeatures.FileManagerIntegration);
        group.AddFeature(
            SufiAIFeatures.PriceMarkupPercent,
            defaultValue: "0",
            displayName: L($"Feature:{SufiAIFeatures.PriceMarkupPercent}"),
            description: L($"Feature:{SufiAIFeatures.PriceMarkupPercent}.Description"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(0)));
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
