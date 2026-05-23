using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.AIManagement.Chat;

public class SendChatMessageInput
{
    [Required]
    public string WorkspaceName { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public List<ChatMessageDto> ConversationHistory { get; set; } = new();
}
