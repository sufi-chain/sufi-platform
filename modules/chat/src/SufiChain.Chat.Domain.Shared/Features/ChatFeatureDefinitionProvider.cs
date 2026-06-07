using SufiChain.Chat.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.Localization;

namespace SufiChain.Chat.Features;

public class ChatFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
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

    protected virtual void AddToggle(FeatureGroupDefinition group, string name)
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
