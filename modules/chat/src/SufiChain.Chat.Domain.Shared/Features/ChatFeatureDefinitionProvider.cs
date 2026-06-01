using SufiChain.Chat.Localization;
using SufiChain.SufiAbp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;
using AbpFeatureDefinitionContext = Volo.Abp.Features.IFeatureDefinitionContext;
using AbpFeatureGroupDefinition = Volo.Abp.Features.FeatureGroupDefinition;

namespace SufiChain.Chat.Features;

public class ChatFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(AbpFeatureDefinitionContext context)
    {
        var group = context.AddGroup(ChatFeatures.GroupName, L("Menu:Chat"));

        AddToggle(group, ChatFeatures.Enable);
        AddToggle(group, ChatFeatures.PublicWidget);
        AddToggle(group, ChatFeatures.Attachments);
        AddToggle(group, ChatFeatures.Realtime);
        AddToggle(group, ChatFeatures.EmailConnector);
        AddToggle(group, ChatFeatures.Ai.Enable);
        AddToggle(group, ChatFeatures.Ai.UsageGuard);
        AddToggle(group, ChatFeatures.Ai.AnonymousHandoff);
    }

    protected virtual void AddToggle(AbpFeatureGroupDefinition group, string name)
    {
        group.AddFeature(
            name,
            defaultValue: "true",
            displayName: L($"Feature:{name}"),
            description: L($"Feature:{name}.Description"),
            valueType: new ToggleStringValueType());
    }

    protected static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
