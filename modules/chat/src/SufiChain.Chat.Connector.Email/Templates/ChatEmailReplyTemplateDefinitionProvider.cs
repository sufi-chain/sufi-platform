using SufiChain.Chat.Localization;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.Chat.Connectors.Email.Templates;

public class ChatEmailReplyTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(
                ChatEmailTemplateNames.Reply,
                displayName: LocalizableString.Create<ChatResource>("EmailConnector:ReplyTemplate"),
                layout: null,
                localizationResource: typeof(ChatResource)
            ).WithVirtualFilePath("/Templates/ChatEmailReply.tpl", isInlineLocalized: false));
    }
}
