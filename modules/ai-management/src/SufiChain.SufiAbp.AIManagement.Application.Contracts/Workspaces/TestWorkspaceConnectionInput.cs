using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.AIManagement.Workspaces;

public class TestWorkspaceConnectionInput
{
    public Guid? WorkspaceId { get; set; }

    [Required]
    [StringLength(256)]
    public string Model { get; set; } = string.Empty;

    [StringLength(512)]
    public string? ApiKey { get; set; }

    [StringLength(512)]
    public string? ApiBaseUrl { get; set; }

    public OpenAIApiMode OpenAIApiMode { get; set; } = OpenAIApiMode.ChatCompletions;
}
