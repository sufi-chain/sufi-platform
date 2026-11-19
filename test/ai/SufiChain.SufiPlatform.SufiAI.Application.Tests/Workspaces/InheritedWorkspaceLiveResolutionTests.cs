using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

/// <summary>
/// An inherited projection resolves keys, routes, and the embedder from the host source.
/// The tenant row is a pointer. A missing or inactive source fails closed.
/// </summary>
public class InheritedWorkspaceLiveResolutionTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private const string StaleKey = "sk-stale-copy";
    private const string LiveKey = "sk-live-rotated";
    private const string StaleEndpoint = "https://stale.example/v1";
    private const string LiveEndpoint = "https://live.example/v1";
    private const string EmbedModel = "nvidia/llama-nemotron-embed-1b-v2";

    private readonly IWorkspaceRepository _workspaces;
    private readonly IWorkspaceAssignmentRepository _assignments;
    private readonly IInheritedWorkspaceProjectionSynchronizer _synchronizer;
    private readonly IInheritedWorkspaceLiveView _liveView;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtime;
    private readonly ICurrentTenant _currentTenant;

    public InheritedWorkspaceLiveResolutionTests()
    {
        _workspaces = GetRequiredService<IWorkspaceRepository>();
        _assignments = GetRequiredService<IWorkspaceAssignmentRepository>();
        _synchronizer = GetRequiredService<IInheritedWorkspaceProjectionSynchronizer>();
        _liveView = GetRequiredService<IInheritedWorkspaceLiveView>();
        _runtime = GetRequiredService<IWorkspaceRuntimeConfigurationResolver>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Host_key_rotation_is_visible_without_resync_and_the_copy_stays_unused()
    {
        var graph = await SeedProjectionAsync();
        await StampCopiedSecretAsync(graph.TenantId, graph.ProjectionId);

        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var host = await _workspaces.GetAsync(graph.HostId, includeDetails: true);
                host.UpdateConfiguration(host.DefaultModel, LiveKey, LiveEndpoint, 3m, 4m);
                host.SetGuardrail(WorkspaceGuardrailPeriod.Daily, 9m);
                foreach (var route in host.ModelConfigurations)
                {
                    route.UpdateConfiguration(
                        route.ModelId,
                        LiveEndpoint,
                        LiveKey,
                        route.Priority,
                        route.OpenAIApiMode,
                        route.InputPrice,
                        route.OutputPrice,
                        route.Dimensions,
                        route.DisplayName,
                        route.IsUserSelectable,
                        route.Description,
                        route.MaxContextTokens,
                        route.InputPriceUnit,
                        route.OutputPriceUnit);
                }

                await _workspaces.UpdateAsync(host, autoSave: true);
            });
        }

        using (_currentTenant.Change(graph.TenantId))
        {
            var resolved = await _runtime.ResolveAsync(graph.ProjectionId, AICapabilityType.ChatCompletion);
            resolved.ApiKey.ShouldBe(LiveKey);
            resolved.ApiEndpoint.ShouldBe(LiveEndpoint);
            resolved.Workspace.Id.ShouldBe(graph.ProjectionId);
            resolved.Workspace.TenantId.ShouldBe(graph.TenantId);
            resolved.Workspace.IsInherited.ShouldBeTrue();
            resolved.ApiKey.ShouldNotBe(StaleKey);

            var view = await _workspaces.GetAsync(graph.ProjectionId, includeDetails: true);
            view.Guardrails.Single(item => item.Period == WorkspaceGuardrailPeriod.Daily).AmountUsd.ShouldBe(9m);
            view.ModelConfigurations.ShouldAllBe(route => route.ApiKey == LiveKey);
            view.ModelConfigurations.Select(route => route.Id).ShouldContain(graph.HostChatRouteId);

            var byProjectionRoute = await _runtime.ResolveAsync(
                graph.ProjectionId,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection
                {
                    ModelConfigurationId = graph.ProjectionChatRouteId,
                    AllowedModelConfigurationIds = new[] { graph.ProjectionChatRouteId }
                });
            byProjectionRoute.ModelConfigurationId.ShouldBe(graph.HostChatRouteId);
            byProjectionRoute.ApiKey.ShouldBe(LiveKey);
            byProjectionRoute.ApiEndpoint.ShouldBe(LiveEndpoint);

            var embedder = await GetRequiredService<IWorkspaceEmbedderResolver>()
                .ResolveAsync(view);
            embedder.ApiKey.ShouldBe(LiveKey);
            embedder.Model.ShouldBe(EmbedModel);
        }

        var stored = await ReadStoredAsync(graph.TenantId, graph.ProjectionId);
        stored.ApiKey.ShouldBe(StaleKey);
        stored.ModelConfigurations.ShouldAllBe(route => route.ApiKey == StaleKey);
    }

    [Fact]
    public async Task Missing_or_inactive_source_fails_closed_with_a_localized_error()
    {
        var missing = await SeedProjectionAsync();
        await StampCopiedSecretAsync(missing.TenantId, missing.ProjectionId);
        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(() => _workspaces.DeleteAsync(missing.HostId, autoSave: true));
        }

        using (_currentTenant.Change(missing.TenantId))
        {
            var error = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                _runtime.ResolveAsync(missing.ProjectionId, AICapabilityType.ChatCompletion));
            error.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceSourceUnavailable);
            error.Data["WorkspaceName"].ShouldBe(missing.ProjectionName);
            AssertNoKey(error);

            var decisions = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                GetRequiredService<ISystemOneDecisionClient>().TryDecideAsync(new SystemOneDecisionRequest
                {
                    WorkspaceId = missing.ProjectionId,
                    State = "hello",
                    Questions = new Dictionary<string, SystemOneQuestion>
                    {
                        ["refund"] = new() { Type = "noul", Instructions = "Is this a refund?" }
                    }
                }));
            decisions.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceSourceUnavailable);
            AssertNoKey(decisions);
        }

        var inactive = await SeedProjectionAsync();
        await StampCopiedSecretAsync(inactive.TenantId, inactive.ProjectionId);
        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var host = await _workspaces.GetAsync(inactive.HostId, includeDetails: true);
                host.Deactivate();
                await _workspaces.UpdateAsync(host, autoSave: true);
            });
        }

        using (_currentTenant.Change(inactive.TenantId))
        {
            var error = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                _runtime.ResolveAsync(inactive.ProjectionId, AICapabilityType.ChatCompletion));
            error.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceSourceInactive);
            AssertNoKey(error);

            var readiness = await GetRequiredService<IWorkspaceAppService>().GetReadinessAsync(inactive.ProjectionId);
            readiness.IsReady.ShouldBeFalse();
            readiness.Capabilities.ShouldContain(item =>
                item.CapabilityType == AICapabilityType.ChatCompletion &&
                item.FailureCode == WorkspaceRuntimeFailureCodes.InheritedSourceInactive);
            readiness.WorkspaceName.ShouldBe(inactive.ProjectionName);
        }
    }

    [Fact]
    public async Task New_projection_and_inherited_copy_store_no_host_key()
    {
        var graph = await SeedProjectionAsync();
        var stored = await ReadStoredAsync(graph.TenantId, graph.ProjectionId);
        stored.ApiKey.ShouldBeNull();
        stored.ModelConfigurations.ShouldAllBe(route => string.IsNullOrEmpty(route.ApiKey));

        var manager = GetRequiredService<WorkspaceManager>();
        using (_currentTenant.Change(graph.TenantId))
        {
            var view = await _workspaces.GetAsync(graph.ProjectionId, includeDetails: true);
            view.ApiKey.ShouldBe(LiveKey);
            var copy = manager.CreateCopy(view, "tenant-copy-" + Guid.NewGuid().ToString("N")[..8], graph.TenantId);
            copy.ApiKey.ShouldBeNull();
            copy.ModelConfigurations.ShouldAllBe(route => string.IsNullOrEmpty(route.ApiKey));
        }

        using (_currentTenant.Change(null))
        {
            var host = await _workspaces.GetAsync(graph.HostId, includeDetails: true);
            var copy = manager.CreateCopy(host, "host-copy-" + Guid.NewGuid().ToString("N")[..8], tenantId: null);
            copy.ApiKey.ShouldBe(LiveKey);
            copy.ModelConfigurations.ShouldContain(route => route.ApiKey == LiveKey);
        }
    }

    [Fact]
    public async Task Seed_and_sql_blank_copied_keys_and_leave_owned_keys()
    {
        var first = await SeedProjectionAsync();
        var second = await SeedProjectionAsync();
        await StampCopiedSecretAsync(first.TenantId, first.ProjectionId);
        await StampCopiedSecretAsync(second.TenantId, second.ProjectionId);
        Guid ownedId = Guid.Empty;
        using (_currentTenant.Change(first.TenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var owned = new Workspace(
                    Guid.NewGuid(),
                    "owned-" + Guid.NewGuid().ToString("N")[..8],
                    AIProviderType.OpenRouter,
                    "openrouter/free",
                    first.TenantId);
                owned.UpdateConfiguration("openrouter/free", "sk-tenant-owned", "https://owned.example/v1");
                await _workspaces.InsertAsync(owned, autoSave: true);
                ownedId = owned.Id;
            });
        }

        using (_currentTenant.Change(first.TenantId))
        {
            await GetRequiredService<InheritedWorkspaceCredentialBlankingDataSeedContributor>()
                .SeedAsync(new Volo.Abp.Data.DataSeedContext(first.TenantId));
            await GetRequiredService<InheritedWorkspaceCredentialBlankingDataSeedContributor>()
                .SeedAsync(new Volo.Abp.Data.DataSeedContext(first.TenantId));
        }

        (await ReadStoredAsync(first.TenantId, first.ProjectionId)).ApiKey.ShouldBeNull();
        (await ReadStoredAsync(first.TenantId, first.ProjectionId)).ModelConfigurations
            .ShouldAllBe(route => string.IsNullOrEmpty(route.ApiKey));
        (await ReadStoredAsync(second.TenantId, second.ProjectionId)).ApiKey.ShouldBe(StaleKey);
        (await ReadStoredAsync(first.TenantId, ownedId)).ApiKey.ShouldBe("sk-tenant-owned");

        await StampCopiedSecretAsync(first.TenantId, first.ProjectionId);
        await WithUnitOfWorkAsync(async services =>
        {
            var db = await services.GetRequiredService<IDbContextProvider<IAIDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync(InheritedWorkspaceCredentialBlanking.WorkspaceSql);
            await db.Database.ExecuteSqlRawAsync(InheritedWorkspaceCredentialBlanking.RouteSql);
        });

        (await ReadStoredAsync(first.TenantId, first.ProjectionId)).ApiKey.ShouldBeNull();
        (await ReadStoredAsync(first.TenantId, first.ProjectionId)).ModelConfigurations
            .ShouldAllBe(route => string.IsNullOrEmpty(route.ApiKey));
        (await ReadStoredAsync(second.TenantId, second.ProjectionId)).ApiKey.ShouldBeNull();
        (await ReadStoredAsync(second.TenantId, second.ProjectionId)).ModelConfigurations
            .ShouldAllBe(route => string.IsNullOrEmpty(route.ApiKey));
        (await ReadStoredAsync(first.TenantId, ownedId)).ApiKey.ShouldBe("sk-tenant-owned");
        using (_currentTenant.Change(null))
        {
            var host = await _workspaces.GetAsync(first.HostId, includeDetails: false);
            host.ApiKey.ShouldBe(LiveKey);
        }
    }

    [Fact]
    public async Task Successful_turn_records_the_tenant_and_publishes_the_debit()
    {
        var graph = await SeedProjectionAsync();
        var publisher = new RecordingPublisher();
        using (_currentTenant.Change(graph.TenantId))
        {
            var resolved = await _runtime.ResolveAsync(graph.ProjectionId, AICapabilityType.ChatCompletion);
            await WithUnitOfWorkAsync(async services =>
            {
                var recorder = new AIUsageRecorder(
                    services.GetRequiredService<IAIUsageLogRepository>(),
                    NullLogger<AIUsageRecorder>.Instance,
                    services.GetRequiredService<IUnitOfWorkManager>(),
                    services.GetRequiredService<ICurrentTenant>(),
                    new[] { publisher });
                recorder.LazyServiceProvider = services.GetRequiredService<IAbpLazyServiceProvider>();
                await recorder.RecordAsync(new AIUsageRecord
                {
                    Configuration = resolved,
                    InputTokens = 1000,
                    OutputTokens = 500,
                    LatencyMs = 12,
                    IsSuccess = true
                });
            });

            var logs = await WithUnitOfWorkAsync(services =>
                services.GetRequiredService<IAIUsageLogRepository>().GetByWorkspaceAsync(graph.ProjectionId));
            logs.Count.ShouldBe(1);
            logs[0].TenantId.ShouldBe(graph.TenantId);
            logs[0].IsSharedWorkspaceUsage.ShouldBeTrue();
            logs[0].IsSuccess.ShouldBeTrue();
            logs[0].IsCostCalculated.ShouldBeTrue();
            logs[0].EstimatedCost.ShouldBeGreaterThan(0);
            publisher.Calls.ShouldBe(1);
            publisher.TenantId.ShouldBe(graph.TenantId);
            publisher.UsageLogId.ShouldBe(logs[0].Id);
            publisher.Cost.ShouldBe(logs[0].EstimatedCost);
        }
    }

    [Fact]
    public async Task Embedder_cache_is_shared_across_tenants_and_dropped_on_host_clear()
    {
        var hostId = Guid.NewGuid();
        var name = "embed-" + hostId.ToString("N")[..8];
        var first = await SeedProjectionAsync(hostId, name);
        var second = await SeedProjectionAsync(hostId, name, reuseHost: true);
        var sync = GetRequiredService<WorkspaceSyncService>();

        using (_currentTenant.Change(first.TenantId))
        {
            var firstGenerator = await sync.GetOrCreateEmbeddingGeneratorAsync(first.ProjectionName);
            ReadBearer(firstGenerator).ShouldBe(LiveKey);

            using (_currentTenant.Change(second.TenantId))
            {
                var secondGenerator = await sync.GetOrCreateEmbeddingGeneratorAsync(second.ProjectionName);
                ReferenceEquals(firstGenerator, secondGenerator).ShouldBeTrue();
            }
        }

        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var host = await _workspaces.GetAsync(hostId, includeDetails: true);
                host.UpdateConfiguration(host.DefaultModel, "sk-live-after-clear", LiveEndpoint, 3m, 4m);
                foreach (var route in host.ModelConfigurations)
                {
                    route.UpdateConfiguration(
                        route.ModelId,
                        LiveEndpoint,
                        "sk-live-after-clear",
                        route.Priority,
                        route.OpenAIApiMode,
                        route.InputPrice,
                        route.OutputPrice,
                        route.Dimensions,
                        route.DisplayName,
                        route.IsUserSelectable,
                        route.Description,
                        route.MaxContextTokens,
                        route.InputPriceUnit,
                        route.OutputPriceUnit);
                }

                await _workspaces.UpdateAsync(host, autoSave: true);
            });

            await sync.ClearWorkspaceCache(name);
        }

        using (_currentTenant.Change(first.TenantId))
        {
            var refreshed = await sync.GetOrCreateEmbeddingGeneratorAsync(first.ProjectionName);
            ReadBearer(refreshed).ShouldBe("sk-live-after-clear");
            using (_currentTenant.Change(second.TenantId))
            {
                var other = await sync.GetOrCreateEmbeddingGeneratorAsync(second.ProjectionName);
                ReferenceEquals(refreshed, other).ShouldBeTrue();
                ReadBearer(other).ShouldBe("sk-live-after-clear");
            }
        }

        using (_currentTenant.Change(null))
        {
            await sync.ClearWorkspaceCache(name);
        }
    }

    [Fact]
    public void Source_errors_are_localized_in_fa_en_ar_es()
    {
        var localizer = GetRequiredService<IStringLocalizer<AIResource>>();
        var expected = new Dictionary<string, string>
        {
            ["en"] = "platform",
            ["fa"] = "پلتفرم",
            ["ar"] = "المنصة",
            ["es"] = "plataforma"
        };

        foreach (var (culture, word) in expected)
        {
            using (CultureHelper.Use(culture))
            {
                var unavailable = localizer[AIErrorCodes.InheritedWorkspaceSourceUnavailable].Value;
                var inactive = localizer[AIErrorCodes.InheritedWorkspaceSourceInactive].Value;
                unavailable.ShouldContain(word);
                inactive.ShouldContain(word);
                unavailable.ShouldNotBe(AIErrorCodes.InheritedWorkspaceSourceUnavailable);
                inactive.ShouldNotBe(AIErrorCodes.InheritedWorkspaceSourceInactive);
                unavailable.ShouldContain("{WorkspaceName}");
            }
        }
    }

    private async Task<ProjectionGraph> SeedProjectionAsync(Guid? hostId = null, string? name = null, bool reuseHost = false)
    {
        var id = hostId ?? Guid.NewGuid();
        var workspaceName = name ?? "live-" + id.ToString("N")[..8];
        var tenantId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var projectionName = workspaceName + "-" + tenantId.ToString("N")[..6];
        Guid hostChatRouteId = Guid.Empty;

        using (_currentTenant.Change(null))
        {
            if (!reuseHost)
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var workspace = new Workspace(id, workspaceName, AIProviderType.OpenRouter, "openrouter/free");
                    workspace.UpdateConfiguration("openrouter/free", LiveKey, LiveEndpoint, 3m, 4m);
                    workspace.SetGuardrail(WorkspaceGuardrailPeriod.Daily, 5m);
                    var chat = workspace.AddModelConfiguration(
                        AICapabilityType.ChatCompletion,
                        "openrouter/free",
                        apiEndpoint: LiveEndpoint,
                        apiKey: LiveKey,
                        priority: 0,
                        inputPrice: 10m,
                        outputPrice: 20m,
                        displayName: "Free",
                        isUserSelectable: true);
                    workspace.AddModelConfiguration(
                        AICapabilityType.Embeddings,
                        EmbedModel,
                        apiEndpoint: LiveEndpoint,
                        apiKey: LiveKey,
                        priority: 0,
                        isUserSelectable: true);
                    workspace.AddModelConfiguration(
                        AICapabilityType.Decisions,
                        "openrouter/free",
                        apiEndpoint: LiveEndpoint,
                        apiKey: LiveKey,
                        priority: 0,
                        isUserSelectable: true);
                    await _workspaces.InsertAsync(workspace, autoSave: true);
                    hostChatRouteId = chat.Id;
                });
            }
            else
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    var host = await _workspaces.GetAsync(id, includeDetails: true);
                    hostChatRouteId = host.ModelConfigurations.Single(item => item.CapabilityType == AICapabilityType.ChatCompletion).Id;
                });
            }

            await WithUnitOfWorkAsync(() => _assignments.InsertAsync(
                new WorkspaceAssignment(assignmentId, tenantId, id, projectionId),
                autoSave: true));

            var source = await _workspaces.GetAsync(id, includeDetails: true);
            await _synchronizer.CreateTenantProjectionAsync(
                source,
                tenantId,
                assignmentId,
                projectionId,
                id,
                projectionName);
        }

        Guid projectionChatRouteId;
        using (_currentTenant.Change(tenantId))
        {
            projectionChatRouteId = (await ReadStoredAsync(tenantId, projectionId))
                .ModelConfigurations.Single(item => item.CapabilityType == AICapabilityType.ChatCompletion).Id;
        }

        return new ProjectionGraph(tenantId, id, projectionId, hostChatRouteId, projectionChatRouteId, projectionName);
    }

    private async Task StampCopiedSecretAsync(Guid tenantId, Guid projectionId)
    {
        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                using (_liveView.Suppress())
                {
                    var stored = await _workspaces.GetAsync(projectionId, includeDetails: true);
                    stored.UpdateConfiguration(stored.DefaultModel, StaleKey, StaleEndpoint);
                    foreach (var route in stored.ModelConfigurations)
                    {
                        route.UpdateConfiguration(
                            route.ModelId,
                            StaleEndpoint,
                            StaleKey,
                            route.Priority,
                            route.OpenAIApiMode,
                            route.InputPrice,
                            route.OutputPrice,
                            route.Dimensions,
                            route.DisplayName,
                            route.IsUserSelectable,
                            route.Description,
                            route.MaxContextTokens,
                            route.InputPriceUnit,
                            route.OutputPriceUnit);
                    }

                    await _workspaces.UpdateAsync(stored, autoSave: true);
                }
            });
        }
    }

    private async Task<Workspace> ReadStoredAsync(Guid tenantId, Guid workspaceId)
    {
        using (_currentTenant.Change(tenantId))
        {
            return await WithUnitOfWorkAsync(async () =>
            {
                using (_liveView.Suppress())
                {
                    return await _workspaces.GetAsync(workspaceId, includeDetails: true);
                }
            });
        }
    }

    private static void AssertNoKey(Volo.Abp.BusinessException error)
    {
        error.Message.ShouldNotContain(StaleKey);
        error.Message.ShouldNotContain(LiveKey);
        foreach (var value in error.Data.Values)
        {
            value?.ToString().ShouldNotContain(StaleKey);
            value?.ToString().ShouldNotContain(LiveKey);
        }
    }

    private static string? ReadBearer(object generator)
    {
        var field = generator.GetType().GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        var client = (HttpClient)field.GetValue(generator)!;
        client.DefaultRequestHeaders.Authorization.ShouldNotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.ShouldBe("Bearer");
        return client.DefaultRequestHeaders.Authorization.Parameter;
    }

    private sealed record ProjectionGraph(
        Guid TenantId,
        Guid HostId,
        Guid ProjectionId,
        Guid HostChatRouteId,
        Guid ProjectionChatRouteId,
        string ProjectionName);

    private sealed class RecordingPublisher : IAiSharedUsageChargePublisher
    {
        public int Calls { get; private set; }

        public Guid TenantId { get; private set; }

        public Guid UsageLogId { get; private set; }

        public decimal Cost { get; private set; }

        public Task PublishAsync(Guid usageLogId, Guid tenantId, decimal estimatedCost, System.Threading.CancellationToken cancellationToken = default)
        {
            Calls++;
            UsageLogId = usageLogId;
            TenantId = tenantId;
            Cost = estimatedCost;
            return Task.CompletedTask;
        }
    }
}
