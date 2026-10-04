using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class InheritedWorkspaceLiveView : IInheritedWorkspaceLiveView, ITransientDependency
{
    private static readonly AsyncLocal<int> SuppressDepth = new();
    private static readonly ConditionalWeakTable<Workspace, object> LiveViews = new();

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
                        !sources.TryGetValue(sourceId, out var source))
                    {
                        continue;
                    }

                    var view = projection.CreateLiveInheritedView(source);
                    if (!ReferenceEquals(view, projection))
                    {
                        LiveViews.TryAdd(view, new object());
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
