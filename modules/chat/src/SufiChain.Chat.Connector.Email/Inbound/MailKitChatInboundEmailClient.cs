using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
using MimeKit;
using SufiChain.Chat.Connectors.Email.Settings;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Connectors.Email.Inbound;

public class MailKitChatInboundEmailClient : IChatInboundEmailClient, ITransientDependency
{
    public virtual async Task<IReadOnlyList<ChatInboundEmailMessage>> FetchUnreadAsync(
        ChatEmailConnectorRuntimeSettings settings,
        CancellationToken cancellationToken = default)
    {
        return settings.InboundProtocol switch
        {
            ChatInboundEmailProtocol.Imap => await FetchFromImapAsync(settings, cancellationToken),
            ChatInboundEmailProtocol.Pop3 => await FetchFromPop3Async(settings, cancellationToken),
            _ => Array.Empty<ChatInboundEmailMessage>()
        };
    }

    protected virtual async Task<IReadOnlyList<ChatInboundEmailMessage>> FetchFromImapAsync(
        ChatEmailConnectorRuntimeSettings settings,
        CancellationToken cancellationToken)
    {
        using var client = new ImapClient();
        await ConnectAsync(client, settings, cancellationToken);
        await client.Inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

        var unreadIds = await client.Inbox.SearchAsync(SearchQuery.NotSeen, cancellationToken);
        var messages = new List<ChatInboundEmailMessage>();

        foreach (var uid in unreadIds)
        {
            var mimeMessage = await client.Inbox.GetMessageAsync(uid, cancellationToken);
            messages.Add(MapMessage(mimeMessage));
            await client.Inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
        }

        await client.DisconnectAsync(true, cancellationToken);
        return messages;
    }

    protected virtual async Task<IReadOnlyList<ChatInboundEmailMessage>> FetchFromPop3Async(
        ChatEmailConnectorRuntimeSettings settings,
        CancellationToken cancellationToken)
    {
        using var client = new Pop3Client();
        await ConnectAsync(client, settings, cancellationToken);

        var messages = new List<ChatInboundEmailMessage>();
        for (var index = 0; index < client.Count; index++)
        {
            var mimeMessage = await client.GetMessageAsync(index, cancellationToken);
            messages.Add(MapMessage(mimeMessage));
            await client.DeleteMessageAsync(index, cancellationToken);
        }

        await client.DisconnectAsync(true, cancellationToken);
        return messages;
    }

    protected virtual async Task ConnectAsync(ImapClient client, ChatEmailConnectorRuntimeSettings settings, CancellationToken cancellationToken)
    {
        await client.ConnectAsync(settings.InboundHost!, settings.InboundPort, settings.InboundUseSsl, cancellationToken);
        await client.AuthenticateAsync(settings.InboundUserName!, settings.InboundPassword!, cancellationToken);
    }

    protected virtual async Task ConnectAsync(Pop3Client client, ChatEmailConnectorRuntimeSettings settings, CancellationToken cancellationToken)
    {
        await client.ConnectAsync(settings.InboundHost!, settings.InboundPort, settings.InboundUseSsl, cancellationToken);
        await client.AuthenticateAsync(settings.InboundUserName!, settings.InboundPassword!, cancellationToken);
    }

    protected virtual ChatInboundEmailMessage MapMessage(MimeMessage mimeMessage)
    {
        return new ChatInboundEmailMessage
        {
            MessageId = ChatEmailThreadResolver.NormalizeMessageId(mimeMessage.MessageId),
            InReplyTo = ChatEmailThreadResolver.NormalizeMessageId(mimeMessage.InReplyTo),
            References = mimeMessage.References?.ToString(),
            FromAddress = mimeMessage.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
            FromName = mimeMessage.From.Mailboxes.FirstOrDefault()?.Name,
            Subject = mimeMessage.Subject ?? string.Empty,
            Body = mimeMessage.TextBody ?? mimeMessage.HtmlBody ?? string.Empty
        };
    }
}
