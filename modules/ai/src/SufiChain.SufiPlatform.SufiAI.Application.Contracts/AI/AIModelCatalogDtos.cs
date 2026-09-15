using System;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI;

public class GetSelectableModelRoutesInput
{
    [Required]
    public Guid WorkspaceId { get; set; }

    public AICapabilityType CapabilityType { get; set; } = AICapabilityType.ChatCompletion;

    public Guid? CopilotId { get; set; }
}

public class AIModelRouteDto
{
    public Guid Id { get; set; }

    public string ModelId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public AICapabilityType CapabilityType { get; set; }

    public OpenAIApiMode OpenAIApiMode { get; set; }

    public bool IsDefault { get; set; }

    public bool IsReady { get; set; }

    public string? UnavailableReason { get; set; }

    public bool SupportsToolCalling { get; set; }
}
