using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.AIManagement.Workspaces;

public class GetOpenAIModelsInput
{
    public Guid? WorkspaceId { get; set; }

    [StringLength(512)]
    public string? ApiKey { get; set; }

    [StringLength(512)]
    public string? ApiBaseUrl { get; set; }
}
