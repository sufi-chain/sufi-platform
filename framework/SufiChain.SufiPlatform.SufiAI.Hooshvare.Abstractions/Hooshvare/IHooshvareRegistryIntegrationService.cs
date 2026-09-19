using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

/// <summary>
/// Cross-module hooshvare registry contract. Prefer this over in-proc <c>IPlatformHooshvareResolver</c> / catalog app services.
/// </summary>
[IntegrationService]
public interface IHooshvareRegistryIntegrationService : IApplicationService
{
    /// <summary>Resolves a runtime hooshvare definition by stable key.</summary>
    Task<HooshvareRegistryRuntimeDto> GetByKeyAsync(string key);

    /// <summary>Resolves a runtime hooshvare id by stable key.</summary>
    Task<Guid> GetIdByKeyAsync(string key);

    /// <summary>Lists registry items optionally filtered by kind / purpose.</summary>
    Task<List<HooshvareRegistryItemDto>> GetListAsync(
        HooshvareKind? kind = null,
        string? purpose = null,
        bool includePublicOnly = false);
}
