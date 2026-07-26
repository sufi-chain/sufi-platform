using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class TestWorkspaceConnectionInput
{
    public Guid? WorkspaceId { get; set; }

    /// <summary>
    /// When set, empty ApiKey/ApiBaseUrl fall back to this model configuration before the workspace.
    /// </summary>
    public Guid? ModelConfigurationId { get; set; }

    /// <summary>
    /// Determines which provider probe to run. Defaults to chat completions for workspace-level tests.
    /// </summary>
    public AICapabilityType CapabilityType { get; set; } = AICapabilityType.ChatCompletion;

    [Required]
    [StringLength(256)]
    public string Model { get; set; } = string.Empty;

    [StringLength(512)]
    public string? ApiKey { get; set; }

    [StringLength(512)]
    public string? ApiBaseUrl { get; set; }

    public OpenAIApiMode OpenAIApiMode { get; set; } = OpenAIApiMode.ChatCompletions;
}
