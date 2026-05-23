namespace SufiChain.SufiAbp.AIManagement.Chat;

public class ChatResponseDto
{
    public string Message { get; set; } = string.Empty;
    public int? TokensUsed { get; set; }
    public string Model { get; set; } = string.Empty;
}
