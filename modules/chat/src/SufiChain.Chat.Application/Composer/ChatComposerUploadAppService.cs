using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Features;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.FileManager.FileItems;
using Volo.Abp;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Composer;

[Authorize]
public class ChatComposerUploadAppService : ChatAppService, IChatComposerUploadAppService
{
    protected IFileItemAppService FileItemAppService { get; }

    protected IChatSessionRepository SessionRepository { get; }

    protected IFeatureChecker FeatureChecker { get; }

    public ChatComposerUploadAppService(
        IFileItemAppService fileItemAppService,
        IChatSessionRepository sessionRepository,
        IFeatureChecker featureChecker)
    {
        FileItemAppService = fileItemAppService;
        SessionRepository = sessionRepository;
        FeatureChecker = featureChecker;
    }

    public virtual async Task<FileItemDto> UploadAsync(ChatComposerUploadInput input)
    {
        await EnsureCanSendMessagesAsync();

        if (!CurrentUser.IsAuthenticated)
        {
            throw new BusinessException(ChatErrorCodes.AttachmentsDisabled);
        }

        if (!await FeatureChecker.IsEnabledAsync(ChatFeatures.Attachments) ||
            !await SettingProvider.IsTrueAsync(ChatSettingNames.General.EnableFileAttachments))
        {
            throw new BusinessException(ChatErrorCodes.AttachmentsDisabled);
        }

        if (input.IsVoiceRecording &&
            !await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableVoiceMessages))
        {
            throw new BusinessException(ChatErrorCodes.VoiceMessagesDisabled);
        }

        await SessionRepository.GetAsync(input.SessionId);

        return await FileItemAppService.UploadAsync(new UploadFileInput
        {
            FileName = input.FileName,
            MimeType = input.MimeType,
            Content = input.Content,
            StructureKey = ChatFileStructureKeys.Attachments,
            EntityType = ChatEntityTypes.Session,
            EntityId = input.SessionId,
            AutoConfirm = true
        });
    }
}
