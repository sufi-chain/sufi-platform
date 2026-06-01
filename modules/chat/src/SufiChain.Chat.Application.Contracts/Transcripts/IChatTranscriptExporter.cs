using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Transcripts;

public interface IChatTranscriptExporter : IApplicationService
{
    Task<ChatTranscriptDto> ExportAsync(Guid sessionId, ChatTranscriptExportOptions? options = null);

    Task<string> ExportAsPlainTextAsync(Guid sessionId, ChatTranscriptExportOptions? options = null);
}
