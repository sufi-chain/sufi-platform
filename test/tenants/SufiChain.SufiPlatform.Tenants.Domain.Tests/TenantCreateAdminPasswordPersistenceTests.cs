using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantCreateAdminPasswordPersistenceTests
{
    private const string AdminEmail = "owner@acme.test";
    private const string AdminPassword = "Dialog-Admin-Pass-94";

    [Fact]
    public async Task Admin_password_is_absent_from_the_outbox_row_and_the_audit_log()
    {
        var tenant = new TestTenant(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "acme");
        var tenants = Substitute.For<ITenantRepository>();
        tenants.InsertAsync(Arg.Any<Tenant>(), true, Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Tenant>());
        tenants.FindAsync(tenant.Id, true, Arg.Any<CancellationToken>()).Returns(tenant);

        var manager = Substitute.For<ITenantManager>();
        manager.CreateAsync(tenant.Name).Returns(tenant);

        var identity = Substitute.For<IIdentityDataSeeder>();
        string seededPassword = string.Empty;
        identity.SeedAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<string>())
            .Returns(call =>
            {
                seededPassword = call.ArgAt<string>(1);
                return new IdentityDataSeedResult { CreatedAdminUser = true };
            });

        var current = new MutableCurrentTenant();
        var seeder = new PasswordRecordingDataSeeder(new IdentityDataSeedContributor(identity));
        var outbox = new RecordingEventOutbox();
        var services = new ServiceCollection();
        services.AddSingleton(outbox);
        services.AddSingleton<IAmbientUnitOfWork, AmbientUnitOfWork>();
        services.AddSingleton<IUnitOfWorkManager, UnitOfWorkManager>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        services.AddTransient<IUnitOfWorkEventPublisher, UnitOfWorkEventPublisher>();
        services.AddSingleton(Options.Create(new AbpUnitOfWorkDefaultOptions()));
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.UtcNow);
        services.AddSingleton(clock);
        services.AddSingleton<ICurrentTenant>(current);
        services.AddSingleton(Substitute.For<IEventHandlerInvoker>());
        services.AddSingleton(Substitute.For<ILocalEventBus>());
        services.AddSingleton(Substitute.For<ICorrelationIdProvider>());
        services.Configure<AbpDistributedEventBusOptions>(options =>
        {
            // Same shape as SufiPlatformEntityFrameworkCoreModule: an outbox is
            // configured for every distributed event. UseDbContext would set
            // ImplementationType to the EF outbox that writes EventData.
            options.Outboxes.Configure(config =>
            {
                config.ImplementationType = typeof(RecordingEventOutbox);
            });
        });
        services.AddSingleton(sp => new LocalDistributedEventBus(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<ICurrentTenant>(),
            sp.GetRequiredService<IUnitOfWorkManager>(),
            sp.GetRequiredService<IOptions<AbpDistributedEventBusOptions>>(),
            SimpleGuidGenerator.Instance,
            sp.GetRequiredService<IClock>(),
            sp.GetRequiredService<IEventHandlerInvoker>(),
            sp.GetRequiredService<ILocalEventBus>(),
            sp.GetRequiredService<ICorrelationIdProvider>()));
        services.AddSingleton<IDistributedEventBus>(sp => sp.GetRequiredService<LocalDistributedEventBus>());

        var authorization = Substitute.For<IAbpAuthorizationService>();
        authorization.AuthorizeAsync(
                Arg.Any<System.Security.Claims.ClaimsPrincipal>(),
                Arg.Any<object>(),
                Arg.Any<string>())
            .Returns(AuthorizationResult.Success());
        services.AddSingleton<IAuthorizationService>(authorization);

        var provider = services.BuildServiceProvider();
        var unitOfWorkManager = provider.GetRequiredService<IUnitOfWorkManager>();
        var unitOfWork = unitOfWorkManager.Begin(new AbpUnitOfWorkOptions());
        var service = new TenantAppService(
            tenants,
            manager,
            seeder,
            provider.GetRequiredService<IDistributedEventBus>(),
            new TenantDedicatedSchemaMigrator(tenants, Array.Empty<ITenantSchemaMigrator>()))
        {
            LazyServiceProvider = new AbpLazyServiceProvider(provider)
        };

        await service.CreateAsync(new TenantCreateDto
        {
            Name = "acme",
            AdminEmailAddress = AdminEmail,
            AdminPassword = AdminPassword
        });
        await unitOfWork.CompleteAsync();

        seededPassword.ShouldBe(AdminPassword);
        seeder.Context[TenantCreatedEto.AdminPasswordPropertyName].ShouldBe(AdminPassword);
        outbox.Events.Count.ShouldBe(1);
        outbox.Events[0].EventName.ShouldBe("SufiChain.SufiPlatform.Tenants.TenantCreated");
        var stored = Encoding.UTF8.GetString(outbox.Events[0].EventData);
        stored.ShouldContain(AdminEmail);
        stored.ShouldNotContain(AdminPassword);
        stored.ShouldNotContain(TenantCreatedEto.AdminPasswordPropertyName);

        var audit = new JsonAuditSerializer(Options.Create(new AbpAuditingOptions())).Serialize(new TenantCreateDto
        {
            Name = "acme",
            AdminEmailAddress = AdminEmail,
            AdminPassword = AdminPassword
        });
        audit.ShouldContain(AdminEmail);
        audit.ShouldNotContain(AdminPassword);
    }

    private sealed class RecordingEventOutbox : IEventOutbox
    {
        public List<OutgoingEventInfo> Events { get; } = new();

        public Task EnqueueAsync(OutgoingEventInfo outgoingEvent)
        {
            Events.Add(outgoingEvent);
            return Task.CompletedTask;
        }

        public Task<List<OutgoingEventInfo>> GetWaitingEventsAsync(
            int maxCount,
            Expression<Func<IOutgoingEventInfo, bool>> filter = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Events);
        }

        public Task DeleteAsync(Guid id)
        {
            return Task.CompletedTask;
        }

        public Task DeleteManyAsync(IEnumerable<Guid> ids)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class PasswordRecordingDataSeeder : IDataSeeder
    {
        private readonly IdentityDataSeedContributor _contributor;

        public PasswordRecordingDataSeeder(IdentityDataSeedContributor contributor)
        {
            _contributor = contributor;
        }

        public DataSeedContext Context { get; private set; }

        public Task SeedAsync(DataSeedContext context)
        {
            Context = context;
            return _contributor.SeedAsync(context);
        }
    }

    private sealed class MutableCurrentTenant : ICurrentTenant
    {
        public bool IsAvailable => Id.HasValue;

        public Guid? Id { get; private set; }

        public string Name { get; private set; }

        public IDisposable Change(Guid? id, string name = null)
        {
            var previousId = Id;
            var previousName = Name;
            Id = id;
            Name = name;
            return new Restore(() =>
            {
                Id = previousId;
                Name = previousName;
            });
        }

        private sealed class Restore : IDisposable
        {
            private readonly Action _restore;

            public Restore(Action restore)
            {
                _restore = restore;
            }

            public void Dispose() => _restore();
        }
    }

    private sealed class TestTenant : Tenant
    {
        public TestTenant(Guid id, string name) : base(id, name, name.ToUpperInvariant())
        {
        }
    }
}
