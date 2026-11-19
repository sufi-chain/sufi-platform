using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class InheritedWorkspaceLiveView : IInheritedWorkspaceLiveView, ITransientDependency
{
    private static readonly AsyncLocal<int> SuppressDepth = new();
    private static readonly ConditionalWeakTable<Workspace, object> LiveViews = new();
    private static readonly ConditionalWeakTable<Workspace, SourceState> ResolutionStates = new();

    private enum SourceKind
    {
        Missing,
        Inactive
    }

    private sealed class SourceState
    {
        public SourceKind Kind { get; init; }
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter _dataFilter;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public InheritedWorkspaceLiveView(
        IServiceProvider serviceProvider,
        ICurrentTenant currentTenant,
        IDataFilter dataFilter,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _serviceProvider = serviceProvider;
        _currentTenant = currentTenant;
        _dataFilter = dataFilter;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public static bool IsLiveView(Workspace workspace)
    {
        return LiveViews.TryGetValue(workspace, out _);
    }

    public static bool IsSourceMissing(Workspace workspace)
    {
        return ResolutionStates.TryGetValue(workspace, out var state) && state.Kind == SourceKind.Missing;
    }

    public static bool IsSourceInactive(Workspace workspace)
    {
        return ResolutionStates.TryGetValue(workspace, out var state) && state.Kind == SourceKind.Inactive;
    }

    /// <summary>
    /// Inherited use must read the host source. A raw tenant copy, a missing source,
    /// or an inactive source fails closed and never returns the copied key.
    /// A projection the tenant turned off stays the ordinary inactive-workspace error.
    /// </summary>
    public static void EnsureUsable(Workspace workspace)
    {
        if (workspace.IsInherited && !IsLiveView(workspace))
        {
            throw Unavailable(workspace);
        }

        if (IsSourceMissing(workspace))
        {
            throw Unavailable(workspace);
        }

        if (IsSourceInactive(workspace))
        {
            throw Inactive(workspace);
        }

        if (workspace.IsInherited && !workspace.IsActive)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotActive)
                .WithData("WorkspaceName", workspace.Name)
                .WithData("WorkspaceId", workspace.Id);
        }
    }

    public static string CanonicalCacheId(Workspace workspace)
    {
        var id = workspace.IsInherited && workspace.SourceWorkspaceId is Guid sourceId
            ? sourceId
            : workspace.Id;
        return id.ToString("N");
    }

    public IDisposable Suppress()
    {
        SuppressDepth.Value++;
        return new SuppressScope();
    }

    public virtual async Task<Workspace> ApplyAsync(
        Workspace workspace,
        bool includeDetails,
        CancellationToken cancellationToken = default)
    {
        if (!ShouldProject(workspace, includeDetails))
        {
            return workspace;
        }

        var views = await CreateViewsAsync(new[] { workspace }, cancellationToken);
        return views.TryGetValue(workspace.Id, out var view) ? view : workspace;
    }

    public virtual async Task<List<Workspace>> ApplyAsync(
        IReadOnlyList<Workspace> workspaces,
        CancellationToken cancellationToken = default)
    {
        if (SuppressDepth.Value > 0 || workspaces.Count == 0)
        {
            return workspaces.ToList();
        }

        var inherited = workspaces
            .Where(workspace => workspace.IsInherited && workspace.SourceWorkspaceId.HasValue)
            .ToList();
        if (inherited.Count == 0)
        {
            return workspaces.ToList();
        }

        var views = await CreateViewsAsync(inherited, cancellationToken);
        var result = new List<Workspace>(workspaces.Count);
        foreach (var workspace in workspaces)
        {
            result.Add(views.TryGetValue(workspace.Id, out var view) ? view : workspace);
        }

        return result;
    }

    private static bool ShouldProject(Workspace workspace, bool includeDetails)
    {
        return SuppressDepth.Value == 0 &&
               includeDetails &&
               workspace.IsInherited &&
               workspace.SourceWorkspaceId.HasValue;
    }

    private async Task<Dictionary<Guid, Workspace>> CreateViewsAsync(
        IReadOnlyList<Workspace> projections,
        CancellationToken cancellationToken)
    {
        var sourceIds = projections
            .Select(workspace => workspace.SourceWorkspaceId!.Value)
            .Distinct()
            .ToList();

        using (_currentTenant.Change(null))
        using (var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
        using (_dataFilter.Disable<IMultiTenant>())
        {
            SuppressDepth.Value++;
            try
            {
                var repository = _serviceProvider.GetRequiredService<IWorkspaceRepository>();
                var sources = new Dictionary<Guid, Workspace>(sourceIds.Count);
                foreach (var sourceId in sourceIds)
                {
                    var source = await repository.FindAsync(
                        sourceId,
                        includeDetails: true,
                        cancellationToken: cancellationToken);
                    if (source != null)
                    {
                        sources[sourceId] = source;
                    }
                }

                var views = new Dictionary<Guid, Workspace>(projections.Count);
                foreach (var projection in projections)
                {
                    if (projection.SourceWorkspaceId is not Guid sourceId ||
                        !sources.TryGetValue(sourceId, out var source) ||
                        source.IsDeleted)
                    {
                        var shell = CreateUnavailableShell(projection);
                        LiveViews.TryAdd(shell, new object());
                        ResolutionStates.TryAdd(shell, new SourceState { Kind = SourceKind.Missing });
                        views[projection.Id] = shell;
                        continue;
                    }

                    var view = projection.CreateLiveInheritedView(source);
                    if (!ReferenceEquals(view, projection))
                    {
                        LiveViews.TryAdd(view, new object());
                        if (!source.IsActive)
                        {
                            ResolutionStates.TryAdd(view, new SourceState { Kind = SourceKind.Inactive });
                        }

                        views[projection.Id] = view;
                    }
                }

                await unitOfWork.CompleteAsync(cancellationToken);
                return views;
            }
            finally
            {
                SuppressDepth.Value--;
            }
        }
    }

    private static Workspace CreateUnavailableShell(Workspace projection)
    {
        var model = string.IsNullOrWhiteSpace(projection.DefaultModel) ? "unavailable" : projection.DefaultModel;
        var shell = new Workspace(projection.Id, projection.Name, projection.Provider, model, projection.TenantId);
        shell.UpdateConfiguration(model, apiKey: null, apiBaseUrl: null);
        shell.Deactivate();
        shell.ClearStoredCredentials();
        if (projection.TenantId != null && projection.SourceWorkspaceId is Guid sourceId)
        {
            shell.MarkAsInherited(sourceId, projection.AssignmentId ?? Guid.Empty);
        }

        return shell;
    }

    private static BusinessException Unavailable(Workspace workspace)
    {
        return new BusinessException(AIErrorCodes.InheritedWorkspaceSourceUnavailable)
            .WithData("WorkspaceName", workspace.Name)
            .WithData("WorkspaceId", workspace.Id);
    }

    private static BusinessException Inactive(Workspace workspace)
    {
        return new BusinessException(AIErrorCodes.InheritedWorkspaceSourceInactive)
            .WithData("WorkspaceName", workspace.Name)
            .WithData("WorkspaceId", workspace.Id);
    }

    private sealed class SuppressScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (SuppressDepth.Value > 0)
            {
                SuppressDepth.Value--;
            }
        }
    }
}
