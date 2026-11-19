using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

/// <summary>
/// AI-31: a tenant reading an inherited host workspace receives HasApiKey, never the host or route key.
/// </summary>
public class InheritedWorkspaceTenantSecretTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private const string HostKey = "sk-host-AI31-7f3c9a";
    private const string RouteKey = "sk-route-AI31-7f3c9a";

    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceAssignmentRepository _assignmentRepository;
    private readonly IInheritedWorkspaceProjectionSynchronizer _synchronizer;
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IAIAppService _aiAppService;
    private readonly ICurrentTenant _currentTenant;

    public InheritedWorkspaceTenantSecretTests()
    {
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _assignmentRepository = GetRequiredService<IWorkspaceAssignmentRepository>();
        _synchronizer = GetRequiredService<IInheritedWorkspaceProjectionSynchronizer>();
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _aiAppService = GetRequiredService<IAIAppService>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Tenant_read_dtos_omit_inherited_host_api_keys()
    {
        var tenantId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var hostWorkspaceId = Guid.NewGuid();

        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var workspace = new Workspace(
                    hostWorkspaceId,
                    "host-secret-" + hostWorkspaceId.ToString("N")[..8],
                    AIProviderType.OpenRouter,
                    "openrouter/free");
                workspace.UpdateConfiguration(
                    "openrouter/free",
                    HostKey,
                    "https://or-gateway.sufichain.com/v1",
                    inputCostPer1MTokens: 1m,
                    outputCostPer1MTokens: 2m);
                workspace.AddModelConfiguration(
                    AICapabilityType.ChatCompletion,
                    "openrouter/free",
                    apiEndpoint: "https://or-gateway.sufichain.com/v1",
                    apiKey: RouteKey,
                    priority: 0,
                    displayName: "Free",
                    isUserSelectable: true);
                await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            });

            await WithUnitOfWorkAsync(() => _assignmentRepository.InsertAsync(
                new WorkspaceAssignment(assignmentId, tenantId, hostWorkspaceId, projectionId),
                autoSave: true));

            var source = await _workspaceRepository.GetAsync(hostWorkspaceId, includeDetails: true);
            await _synchronizer.CreateTenantProjectionAsync(
                source,
                tenantId,
                assignmentId,
                projectionId,
                hostWorkspaceId,
                source.Name);
        }

        WorkspaceDto tenantWorkspace;
        WorkspaceReadinessDto readiness;
        List<AIModelConfigurationDto> routes;
        using (_currentTenant.Change(tenantId))
        {
            tenantWorkspace = await _workspaceAppService.GetAsync(projectionId);
            readiness = await _workspaceAppService.GetReadinessAsync(projectionId);
            routes = await _aiAppService.GetModelConfigurationsAsync(projectionId);
            var list = await _workspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 50
            });
            var lookup = await _workspaceAppService.GetLookupAsync();

            tenantWorkspace.HasApiKey.ShouldBeTrue();
            tenantWorkspace.IsInherited.ShouldBeTrue();
            routes.ShouldNotBeEmpty();
            routes.ShouldAllBe(route => route.HasApiKey);
            readiness.Capabilities.ShouldContain(item => item.HasApiKey);

            AssertNoSecret(tenantWorkspace, readiness, routes, list, lookup);
        }

        using (_currentTenant.Change(null))
        {
            var hostWorkspace = await _workspaceAppService.GetAsync(hostWorkspaceId);
            var hostRoutes = await _aiAppService.GetModelConfigurationsAsync(hostWorkspaceId);
            hostWorkspace.HasApiKey.ShouldBeTrue();
            hostRoutes.ShouldAllBe(route => route.HasApiKey);
            AssertNoSecret(hostWorkspace, hostRoutes);
        }
    }

    private static void AssertNoSecret(params object[] values)
    {
        var json = JsonSerializer.Serialize(values);
        json.ShouldNotContain(HostKey);
        json.ShouldNotContain(RouteKey);

        foreach (var text in Walk(values))
        {
            text.ShouldNotContain(HostKey);
            text.ShouldNotContain(RouteKey);
        }
    }

    private static IEnumerable<string> Walk(object? value)
    {
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return Walk(value, seen);
    }

    private static IEnumerable<string> Walk(object? value, HashSet<object> seen)
    {
        if (value is null || value is string || value.GetType().IsValueType)
        {
            if (value is string text)
            {
                yield return text;
            }

            yield break;
        }

        if (!seen.Add(value))
        {
            yield break;
        }

        if (value is IEnumerable sequence)
        {
            foreach (var item in sequence)
            {
                foreach (var text in Walk(item, seen))
                {
                    yield return text;
                }
            }

            yield break;
        }

        foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead)
            {
                continue;
            }

            object? child;
            try
            {
                child = property.GetValue(value);
            }
            catch (TargetInvocationException)
            {
                continue;
            }

            foreach (var text in Walk(child, seen))
            {
                yield return text;
            }
        }
    }
}
