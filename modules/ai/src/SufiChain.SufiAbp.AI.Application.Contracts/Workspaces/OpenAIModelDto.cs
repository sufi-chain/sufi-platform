namespace SufiChain.SufiAbp.AI.Workspaces;

public class OpenAIModelDto
{
    public string Id { get; set; } = string.Empty;

    public string? OwnedBy { get; set; }

    public long? Created { get; set; }
}
