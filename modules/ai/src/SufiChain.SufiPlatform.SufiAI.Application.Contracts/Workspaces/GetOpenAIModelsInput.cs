using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class GetOpenAIModelsInput
{
    public Guid? WorkspaceId { get; set; }

    /// <summary>
    /// When set, empty ApiKey/ApiBaseUrl fall back to this model configuration before the workspace.
    /// </summary>
    public Guid? ModelConfigurationId { get; set; }

    [StringLength(512)]
    public string? ApiKey { get; set; }

    [StringLength(512)]
    public string? ApiBaseUrl { get; set; }
}
