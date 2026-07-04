using SufiChain.SufiAbp.Communications.Localization;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.Communications.Templates;

public class StandardMessageTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(
                StandardMessageTemplates.Layout,
                displayName: LocalizableString.Create<CommunicationsResource>("TextTemplate:Layout"),
                layout: null,
                localizationResource: typeof(CommunicationsResource)
            ).WithVirtualFilePath("/Templates/Layout", isInlineLocalized: true),
            
            new TemplateDefinition(
                StandardMessageTemplates.Message,
                displayName: LocalizableString.Create<CommunicationsResource>("TextTemplate:Message"),
                layout: StandardMessageTemplates.Layout,
                localizationResource: typeof(CommunicationsResource)
            ).WithVirtualFilePath("/Templates/Message", isInlineLocalized: true)
        );
    }
}
