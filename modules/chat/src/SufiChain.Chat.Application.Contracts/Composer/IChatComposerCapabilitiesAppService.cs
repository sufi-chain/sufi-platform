using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Composer;

public class ChatComposerCapabilitiesDto
{
    public bool CanUseRichComposer { get; set; }

    public bool CanAttachFiles { get; set; }

    public bool CanShareLocation { get; set; }

    public bool CanRecordVoice { get; set; }

    public bool CanPickFromGallery { get; set; }

    public bool CanUseOperatorCopilot { get; set; }

    public int MaxFilesPerMessage { get; set; }

    public int MaxVoiceRecordingSeconds { get; set; }

    public ChatAttachmentAllowedFileTypes AllowedFileTypes { get; set; }
}

public interface IChatComposerCapabilitiesAppService : IApplicationService
{
    Task<ChatComposerCapabilitiesDto> GetAsync(Guid? sessionId = null);
}
