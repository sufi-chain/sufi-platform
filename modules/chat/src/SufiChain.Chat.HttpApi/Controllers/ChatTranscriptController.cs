using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Transcripts;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/transcripts")]
public class ChatTranscriptController : ChatController, IChatTranscriptExporter
{
    private readonly IChatTranscriptExporter _transcriptExporter;

    public ChatTranscriptController(IChatTranscriptExporter transcriptExporter)
    {
        _transcriptExporter = transcriptExporter;
    }

    [HttpPost("{sessionId}/export")]
    [Authorize(ChatPermissions.Messages.Default)]
    public virtual Task<ChatTranscriptDto> ExportAsync(Guid sessionId, [FromBody] ChatTranscriptExportOptions? options = null)
    {
        return _transcriptExporter.ExportAsync(sessionId, options);
    }

    [HttpPost("{sessionId}/export/plain-text")]
    [Authorize(ChatPermissions.Messages.Default)]
    public virtual Task<string> ExportAsPlainTextAsync(Guid sessionId, [FromBody] ChatTranscriptExportOptions? options = null)
    {
        return _transcriptExporter.ExportAsPlainTextAsync(sessionId, options);
    }
}
