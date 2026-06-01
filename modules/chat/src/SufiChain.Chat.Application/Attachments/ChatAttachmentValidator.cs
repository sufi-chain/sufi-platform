using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.FileManager.FileItems;
using Volo.Abp;

namespace SufiChain.Chat.Attachments;

[Authorize(ChatPermissions.Messages.Send)]
public class ChatAttachmentValidator : ChatAppService, IChatAttachmentValidator
{
    protected IFileItemAppService FileItemAppService { get; }

    public ChatAttachmentValidator(IFileItemAppService fileItemAppService)
    {
        FileItemAppService = fileItemAppService;
    }

    public virtual async Task<ChatAttachmentValidationResult> ValidateAsync(
        Guid sessionId,
        IReadOnlyList<Guid> attachmentFileIds,
        CancellationToken cancellationToken = default)
    {
        if (attachmentFileIds.Count == 0)
        {
            return new ChatAttachmentValidationResult();
        }

        var distinctIds = attachmentFileIds.Distinct().ToList();
        long totalBytes = 0;

        foreach (var attachmentFileId in distinctIds)
        {
            var file = await FileItemAppService.GetAsync(attachmentFileId);

            if (!string.Equals(file.StructureKey, ChatFileStructureKeys.Attachments, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(file.EntityType, ChatEntityTypes.Session, StringComparison.OrdinalIgnoreCase) ||
                file.EntityId != sessionId)
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
