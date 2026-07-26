using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

/// <summary>
/// Cross-module copilot registry contract. Prefer this over in-proc <c>IPlatformCopilotResolver</c> / catalog app services.
/// </summary>
[IntegrationService]
public interface ICopilotRegistryIntegrationService : IApplicationService
{
    /// <summary>Resolves a runtime copilot definition by stable key.</summary>
    Task<CopilotRegistryRuntimeDto> GetByKeyAsync(string key);

    /// <summary>Resolves a runtime copilot id by stable key.</summary>
    Task<Guid> GetIdByKeyAsync(string key);

    /// <summary>Lists registry items optionally filtered by kind / purpose.</summary>
    Task<List<CopilotRegistryItemDto>> GetListAsync(
        CopilotKind? kind = null,
        string? purpose = null,
        bool includePublicOnly = false);
}
