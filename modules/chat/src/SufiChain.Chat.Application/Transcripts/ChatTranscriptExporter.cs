using System.Text;
using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Mapping;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;
using SufiChain.SufiAbp.FileManager.FileItems;
using Volo.Abp.Authorization.Permissions;

namespace SufiChain.Chat.Transcripts;

[Authorize(ChatPermissions.Messages.Default)]
public class ChatTranscriptExporter : ChatAppService, IChatTranscriptExporter
{
    protected IChatSessionRepository SessionRepository { get; }
    protected IChatParticipantRepository ParticipantRepository { get; }
    protected IChatMessageRepository MessageRepository { get; }
    protected IConversationLinkRepository ConversationLinkRepository { get; }
    protected ChatApplicationMapper Mapper { get; }
    protected IPermissionChecker PermissionChecker { get; }
    protected IFileItemAppService FileItemAppService { get; }

    public ChatTranscriptExporter(
        IChatSessionRepository sessionRepository,
        IChatParticipantRepository participantRepository,
        IChatMessageRepository messageRepository,
        IConversationLinkRepository conversationLinkRepository,
        ChatApplicationMapper mapper,
        IPermissionChecker permissionChecker,
        IFileItemAppService fileItemAppService)
    {
        SessionRepository = sessionRepository;
        ParticipantRepository = participantRepository;
        MessageRepository = messageRepository;
        ConversationLinkRepository = conversationLinkRepository;
        Mapper = mapper;
        PermissionChecker = permissionChecker;
        FileItemAppService = fileItemAppService;
    }

    public virtual async Task<ChatTranscriptDto> ExportAsync(Guid sessionId, ChatTranscriptExportOptions? options = null)
    {
        options ??= new ChatTranscriptExportOptions();
        var session = await SessionRepository.GetAsync(sessionId);
        var participants = await ParticipantRepository.GetListBySessionAsync(sessionId);
        var includeInternal = options.IncludeInternalMessages && await PermissionChecker.IsGrantedAsync(ChatPermissions.Messages.ViewInternal);
        var messages = await MessageRepository.GetListBySessionAsync(sessionId, includeInternal, 0, int.MaxValue);

        if (options.From.HasValue)
        {
            messages = messages.Where(message => message.CreationTime >= options.From.Value).ToList();
        }

        if (options.To.HasValue)
        {
            messages = messages.Where(message => message.CreationTime <= options.To.Value).ToList();
        }

        var links = options.IncludeLinks
            ? await ConversationLinkRepository.GetListBySessionAsync(sessionId)
            : new List<ConversationLink>();

        return new ChatTranscriptDto
        {
            Session = Mapper.ToDto(session, participants),
            Messages = messages.Select(Mapper.ToDto).ToList(),
            Links = links.Select(Mapper.ToDto).ToList(),
            ExportedAt = Clock.Now
        };
    }

    public virtual async Task<string> ExportAsPlainTextAsync(Guid sessionId, ChatTranscriptExportOptions? options = null)
    {
        var transcript = await ExportAsync(sessionId, options);
        var builder = new StringBuilder();
        builder.AppendLine(transcript.Session.Title ?? transcript.Session.Id.ToString("D"));

        foreach (var message in transcript.Messages.OrderBy(message => message.CreationTime))
        {
            builder.AppendLine($"[{message.CreationTime:u}] {message.SenderKind}: {message.Body}");

            if (message.AttachmentFileIds.Count > 0)
            {
                var attachmentRefs = await FormatAttachmentRefsAsync(message.AttachmentFileIds);
                builder.AppendLine($"  Attachments: {attachmentRefs}");
            }
        }

        return builder.ToString();
    }

    protected virtual async Task<string> FormatAttachmentRefsAsync(IReadOnlyList<Guid> attachmentFileIds)
    {
        var refs = new List<string>();

        foreach (var fileId in attachmentFileIds)
        {
            var file = await FileItemAppService.GetAsync(fileId);
            refs.Add($"{file.OriginalName} ({fileId:D})");
        }

        return string.Join(", ", refs);
    }
}
