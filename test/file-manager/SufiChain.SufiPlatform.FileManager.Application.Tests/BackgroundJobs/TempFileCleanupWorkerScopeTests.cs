using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager.BackgroundJobs;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.BackgroundJobs;

/// <summary>
/// SMK-17: each host/tenant cleanup pass must use its own DI scope so completing one
/// unit of work cannot dispose FileManagerDbContext for the next tenant.
/// </summary>
public class TempFileCleanupWorkerScopeTests
{
    [Fact]
    public async Task DoWorkAsync_creates_a_distinct_scope_for_host_and_each_tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var tenants = await CreateTenantsAsync(tenantA, tenantB);
        var scopes = new RecordingScopeFactory(tenants);
        var root = new RootServiceProvider(scopes, tenants);
        var worker = new ProbeWorker(scopes);

        await worker.RunAsync(root);

        scopes.CreatedScopes.ShouldBe(3);
        worker.CleanupProviders.Count.ShouldBe(3);
        worker.CleanupProviders.Distinct().Count().ShouldBe(3);
        worker.CleanupTenantIds.ShouldBe(new Guid?[] { null, tenantA, tenantB });
    }

    [Fact]
    public async Task DoWorkAsync_still_runs_one_scoped_cleanup_without_tenants()
    {
        var scopes = new RecordingScopeFactory(tenants: null);
        var root = new RootServiceProvider(scopes, tenants: null);
        var worker = new ProbeWorker(scopes);

        await worker.RunAsync(root);

        scopes.CreatedScopes.ShouldBe(1);
        worker.CleanupProviders.Count.ShouldBe(1);
        worker.CleanupTenantIds.ShouldBe(new Guid?[] { null });
    }

    private static async Task<List<Tenant>> CreateTenantsAsync(params Guid[] tenantIds)
    {
        var manager = new TenantManager(
            Substitute.For<ITenantValidator>(),
            Substitute.For<ITenantNormalizer>(),
            Substitute.For<ILocalEventBus>(),
            Substitute.For<ITenantRepository>());

        var tenants = new List<Tenant>();
        foreach (var tenantId in tenantIds)
        {
            tenants.Add(await manager.CreateAsync(tenantId, "tenant-" + tenantId.ToString("N")));
        }

        return tenants;
    }

    private sealed class ProbeWorker : TempFileCleanupWorker
    {
        public ProbeWorker(IServiceScopeFactory scopeFactory)
            : base(new AbpAsyncTimer(), scopeFactory)
        {
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetService<ILogger>(Arg.Any<Func<IServiceProvider, object>>()).Returns(NullLogger.Instance);
            LazyServiceProvider = lazy;
        }

        public List<IServiceProvider> CleanupProviders { get; } = new();

        public List<Guid?> CleanupTenantIds { get; } = new();

        public Task RunAsync(IServiceProvider services)
        {
            return DoWorkAsync(new PeriodicBackgroundWorkerContext(services));
        }

        protected override Task<(int Deleted, int Failed)> CleanupTempFilesForCurrentContextAsync(
            IServiceProvider serviceProvider)
        {
            CleanupProviders.Add(serviceProvider);
            CleanupTenantIds.Add(serviceProvider.GetRequiredService<ICurrentTenant>().Id);
            return Task.FromResult((0, 0));
        }
    }

    private sealed class RecordingScopeFactory : IServiceScopeFactory
    {
        private readonly List<Tenant>? _tenants;

        public RecordingScopeFactory(List<Tenant>? tenants) => _tenants = tenants;

        public int CreatedScopes { get; private set; }

        public IServiceScope CreateScope()
        {
            CreatedScopes++;
            return new Scope(new ScopeServiceProvider(new StackTenant(), _tenants));
        }

        private sealed class Scope : IServiceScope
        {
            public Scope(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

            public IServiceProvider ServiceProvider { get; }

            public void Dispose()
            {
            }
        }
    }

    private sealed class ScopeServiceProvider : IServiceProvider
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly ITenantRepository? _tenantRepository;

        public ScopeServiceProvider(ICurrentTenant currentTenant, List<Tenant>? tenants)
        {
            _currentTenant = currentTenant;
            if (tenants == null)
            {
                return;
            }

            _tenantRepository = Substitute.For<ITenantRepository>();
            _tenantRepository
                .GetListAsync(
                    Arg.Any<string>(),
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Any<string>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(tenants);
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ICurrentTenant))
            {
                return _currentTenant;
            }

            if (serviceType == typeof(ITenantRepository))
            {
                return _tenantRepository;
            }

            return null;
        }
    }

    private sealed class RootServiceProvider : IServiceProvider
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITenantRepository? _tenantRepository;
        private readonly IFeatureChecker _features;

        public RootServiceProvider(IServiceScopeFactory scopeFactory, List<Tenant>? tenants)
        {
            _scopeFactory = scopeFactory;
            _features = Substitute.For<IFeatureChecker>();
            _features.IsEnabledAsync(SufiFileManagerFeatures.Enable).Returns(true);
            _features.IsEnabledAsync(SufiFileManagerFeatures.FileItems).Returns(true);

            if (tenants == null)
            {
                _tenantRepository = null;
                return;
            }

            _tenantRepository = Substitute.For<ITenantRepository>();
            _tenantRepository
                .GetListAsync(
                    Arg.Any<string>(),
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Any<string>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(tenants);
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IFeatureChecker))
            {
                return _features;
            }

            if (serviceType == typeof(IServiceScopeFactory))
            {
                return _scopeFactory;
            }

            if (serviceType == typeof(ITenantRepository))
            {
                return _tenantRepository;
            }

            return null;
        }
    }

    private sealed class StackTenant : ICurrentTenant
    {
        private readonly Stack<(Guid? Id, string? Name)> _stack = new();

        public bool IsAvailable => Id.HasValue;

        public Guid? Id => _stack.Count == 0 ? null : _stack.Peek().Id;

        public string? Name => _stack.Count == 0 ? null : _stack.Peek().Name;

        public IDisposable Change(Guid? id, string? name = null)
        {
            _stack.Push((id, name));
            return new Popper(_stack);
        }

        private sealed class Popper : IDisposable
        {
            private readonly Stack<(Guid? Id, string? Name)> _stack;
            private bool _disposed;

            public Popper(Stack<(Guid? Id, string? Name)> stack) => _stack = stack;

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                if (_stack.Count > 0)
                {
                    _stack.Pop();
                }
            }
        }
    }
}
