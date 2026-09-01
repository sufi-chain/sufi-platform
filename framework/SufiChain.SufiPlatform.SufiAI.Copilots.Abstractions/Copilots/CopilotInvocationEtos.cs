using SufiChain.SufiPlatform.EventBus;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

/// <summary>
/// Request that a platform copilot produce a response (async invocation path).
/// </summary>
[Serializable]
[EventName("SufiAI.Copilots.InvocationRequested")]
public class CopilotInvocationRequestedEto : SufiIntegrationEto
{
    public string CopilotKey { get; set; } = string.Empty;

    public Guid? CopilotId { get; set; }

    public string Message { get; set; } = string.Empty;

    public Guid? SessionId { get; set; }

    public string? LinkedEntityType { get; set; }

    public string? LinkedEntityId { get; set; }

    public string? MetadataJson { get; set; }
}

/// <summary>
/// Copilot response ready for the requesting feature module.
/// </summary>
[Serializable]
[EventName("SufiAI.Copilots.ResponseReady")]
public class CopilotResponseReadyEto : SufiIntegrationEto
{
    public string CopilotKey { get; set; } = string.Empty;

    public Guid CopilotId { get; set; }

    public Guid? SessionId { get; set; }

    public string ResponseText { get; set; } = string.Empty;

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

}
