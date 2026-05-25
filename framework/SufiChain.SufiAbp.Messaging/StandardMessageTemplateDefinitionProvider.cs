using SufiChain.SufiAbp.Messaging.Localization;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.Messaging.Templates;

public class StandardMessageTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(
                StandardMessageTemplates.Layout,
                displayName: LocalizableString.Create<MessagingResource>("TextTemplate:Layout"),
                layout: null,
                localizationResource: typeof(MessagingResource)
            ).WithVirtualFilePath("/Templates/Layout.tpl", isInlineLocalized: true),
            
            new TemplateDefinition(
                StandardMessageTemplates.Message,
                displayName: LocalizableString.Create<MessagingResource>("TextTemplate:Message"),
                layout: StandardMessageTemplates.Layout,
                localizationResource: typeof(MessagingResource)
            ).WithVirtualFilePath("/Templates/Message.tpl", isInlineLocalized: true)
        );
    }
}
