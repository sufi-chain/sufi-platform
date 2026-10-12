using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Data;

/// <summary>
/// Idempotent seed that rewrites model-route priorities to contiguous <c>0..n-1</c>
/// per capability while keeping today's enabled primary at position 0.
/// No schema change; safe to run on every data seed.
/// </summary>
public class ModelRoutePriorityNormalizer : IDataSeedContributor, ITransientDependency
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<ModelRoutePriorityNormalizer> _logger;

    public ModelRoutePriorityNormalizer(
        IWorkspaceRepository workspaceRepository,
        ICurrentTenant currentTenant,
        ILogger<ModelRoutePriorityNormalizer> logger)
    {
        _workspaceRepository = workspaceRepository;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            var workspaces = await _workspaceRepository.GetListAsync(maxResultCount: int.MaxValue);

            foreach (var workspace in workspaces)
            {
                if (workspace.IsInherited || workspace.ModelConfigurations.Count == 0)
                {
                    continue;
                }

                var before = workspace.ModelConfigurations
                    .Select(configuration => (
                        configuration.Id,
                        configuration.CapabilityType,
                        configuration.ModelId,
                        configuration.Priority))
                    .ToList();

                if (!workspace.NormalizeRoutePriorities())
                {
                    continue;
                }

                LogPriorityChanges(workspace.Id, before, workspace.ModelConfigurations);
                await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
            }
        }
    }

    private void LogPriorityChanges(
        Guid workspaceId,
        IReadOnlyList<(Guid Id, AICapabilityType CapabilityType, string ModelId, int Priority)> before,
        IReadOnlyCollection<AIModelConfiguration> after)
    {
        var afterById = after.ToDictionary(configuration => configuration.Id);
        foreach (var group in before.GroupBy(item => item.CapabilityType))
        {
            var changes = group
                .Where(item => afterById.TryGetValue(item.Id, out var current) && current.Priority != item.Priority)
                .Select(item =>
                {
                    var current = afterById[item.Id];
                    return $"{item.ModelId}:{item.Priority}->{current.Priority}";
                })
                .ToList();

            if (changes.Count == 0)
            {
                continue;
            }

            _logger.LogInformation(
                "Normalized AI model route priorities for workspace {WorkspaceId} capability {Capability}: {Changes}",
                workspaceId,
                group.Key,
                string.Join(", ", changes));
        }
    }
}
