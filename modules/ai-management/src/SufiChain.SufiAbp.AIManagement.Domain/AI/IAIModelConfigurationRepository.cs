using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.AIManagement.AI;

public interface IAIModelConfigurationRepository : IRepository<AIModelConfiguration, Guid>
{
    /// <summary>
    /// Get all configurations for a specific workspace
    /// </summary>
    Task<List<AIModelConfiguration>> GetByWorkspaceIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get enabled configurations for a workspace and capability type, ordered by priority
    /// </summary>
    Task<List<AIModelConfiguration>> GetEnabledByCapabilityAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the highest priority enabled configuration for a capability
    /// </summary>
    Task<AIModelConfiguration?> GetPrimaryConfigurationAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default);
}
