using SufiChain.SufiPlatform.SufiCom.Localization;
using SufiChain.SufiPlatform.TextTemplating;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.SufiCom.Templates;

public class StandardMessageTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(
                StandardMessageTemplates.Layout,
                displayName: LocalizableString.Create<SufiComResource>("TextTemplate:Layout"),
                layout: null,
                localizationResource: typeof(SufiComResource)
            ).WithVirtualFilePath("/Templates/Layout", isInlineLocalized: true),
            
            new TemplateDefinition(
                StandardMessageTemplates.Message,
                displayName: LocalizableString.Create<SufiComResource>("TextTemplate:Message"),
                layout: StandardMessageTemplates.Layout,
                localizationResource: typeof(SufiComResource)
            ).WithVirtualFilePath("/Templates/Message", isInlineLocalized: true)
        );
    }
}
