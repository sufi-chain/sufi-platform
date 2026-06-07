using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.FileManager.FileItems;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Attachments;

[Authorize]
public class ChatAttachmentValidator : ChatAppService, IChatAttachmentValidator
{
    protected IFileItemAppService FileItemAppService { get; }

    protected IPermissionChecker PermissionChecker { get; }

    public ChatAttachmentValidator(
        IFileItemAppService fileItemAppService,
        IPermissionChecker permissionChecker)
    {
        FileItemAppService = fileItemAppService;
        PermissionChecker = permissionChecker;
    }

    public virtual async Task<ChatAttachmentValidationResult> ValidateAsync(
        Guid sessionId,
        IReadOnlyList<Guid> attachmentFileIds,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanSendMessagesAsync();

        if (attachmentFileIds.Count == 0)
        {
            return new ChatAttachmentValidationResult();
        }

        var allowOperatorGallery =
            await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableOperatorGallery) &&
            await PermissionChecker.IsGrantedAsync(ChatPermissions.Inbox.Reply);

        var distinctIds = attachmentFileIds.Distinct().ToList();
        long totalBytes = 0;

        foreach (var attachmentFileId in distinctIds)
        {
            var file = await FileItemAppService.GetAsync(attachmentFileId);

            if (!string.Equals(file.StructureKey, ChatFileStructureKeys.Attachments, StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(ChatErrorCodes.InvalidAttachment);
            }

            if (!allowOperatorGallery &&
                (!string.Equals(file.EntityType, ChatEntityTypes.Session, StringComparison.OrdinalIgnoreCase) ||
                 file.EntityId != sessionId))
            {
                throw new BusinessException(ChatErrorCodes.InvalidAttachment);
            }

            totalBytes += file.Size;
        }

        return new ChatAttachmentValidationResult
        {
            Count = distinctIds.Count,
            TotalBytes = totalBytes
        };
    }
}
