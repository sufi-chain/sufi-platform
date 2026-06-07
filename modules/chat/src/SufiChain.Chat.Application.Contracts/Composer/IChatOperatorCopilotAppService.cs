using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Composer;

public enum ChatOperatorCopilotOperation
{
    Rewrite,
    ImproveTone,
    GenerateFromPrompt
}

public class ChatOperatorCopilotInput
{
    [Required]
    public Guid SessionId { get; set; }

    public string? DraftText { get; set; }

    public string? Prompt { get; set; }

    public ChatOperatorCopilotOperation Operation { get; set; } = ChatOperatorCopilotOperation.Rewrite;
}

public class ChatOperatorCopilotResultDto
{
    public string SuggestedText { get; set; } = string.Empty;

    public string? WorkspaceName { get; set; }

    public int? TotalTokens { get; set; }
}

public interface IChatOperatorCopilotAppService : IApplicationService
{
    Task<ChatOperatorCopilotResultDto> AssistAsync(ChatOperatorCopilotInput input);
}
