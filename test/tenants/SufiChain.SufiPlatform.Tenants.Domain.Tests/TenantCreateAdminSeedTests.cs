using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Authorization;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantCreateAdminSeedTests
{
    private const string AdminEmail = "owner@acme.test";
    private const string AdminPassword = "Dialog-Admin-Pass-94";

    [Fact]
    public async Task Creating_a_tenant_seeds_an_admin_with_that_email_and_a_working_password()
    {
        var tenant = new TestTenant(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "acme");
        var harness = new Harness(tenant);
        var created = harness.CaptureAdmin();

        var result = await harness.Service.CreateAsync(new TenantCreateDto
        {
            Name = "acme",
            AdminEmailAddress = AdminEmail,
            AdminPassword = AdminPassword
        });

        result.Id.ShouldBe(tenant.Id);
        result.Name.ShouldBe("acme");
        harness.Steps.ShouldBe(new[] { "seed" });
        harness.Seeder.SeenTenantId.ShouldBe(tenant.Id);
        harness.Seeder.Context.TenantId.ShouldBe(tenant.Id);
        harness.Seeder.Context[TenantCreatedEto.AdminEmailPropertyName].ShouldBe(AdminEmail);
        harness.Seeder.Context[TenantCreatedEto.AdminPasswordPropertyName].ShouldBe(AdminPassword);
        TenantCreatedEto.AdminEmailPropertyName.ShouldBe(IdentityDataSeedContributor.AdminEmailPropertyName);
        TenantCreatedEto.AdminPasswordPropertyName.ShouldBe(IdentityDataSeedContributor.AdminPasswordPropertyName);

        created.User.ShouldNotBeNull();
        created.User.Email.ShouldBe(AdminEmail);
        created.User.UserName.ShouldBe(IdentityDataSeedConsts.AdminUserNameDefaultValue);
        created.User.TenantId.ShouldBe(tenant.Id);
        created.Hasher.VerifyHashedPassword(created.User, created.Hash, AdminPassword)
            .ShouldBe(PasswordVerificationResult.Success);
        created.Hasher.VerifyHashedPassword(created.User, created.Hash, AdminPassword + "x")
            .ShouldBe(PasswordVerificationResult.Failed);

        await harness.Bus.Received(1).PublishAsync(
            Arg.Is<TenantCreatedEto>(eto =>
                eto.Id == tenant.Id &&
                eto.Name == "acme" &&
                eto.Properties[TenantCreatedEto.AdminEmailPropertyName] == AdminEmail &&
                !eto.Properties.ContainsKey(TenantCreatedEto.AdminPasswordPropertyName)),
            true,
            true);
    }

    [Fact]
    public void Audit_serialization_omits_the_admin_password()
    {
        var serializer = new JsonAuditSerializer(Options.Create(new AbpAuditingOptions()));
        var dto = new TenantCreateDto
        {
            Name = "acme",
            AdminEmailAddress = AdminEmail,
            AdminPassword = AdminPassword
        };

        var audit = serializer.Serialize(dto);

        audit.ShouldContain(AdminEmail);
        audit.ShouldNotContain(AdminPassword);
    }

    [Fact]
    public async Task Dedicated_connection_string_is_migrated_before_the_admin_is_seeded()
    {
        var tenant = new TestTenant(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "dedicated");
        tenant.SetDefaultConnectionString("Server=127.0.0.1;Database=dedicated_acme;Trusted_Connection=True");
        var harness = new Harness(tenant);

        await harness.Service.CreateAsync(new TenantCreateDto
        {
            Name = "dedicated",
            AdminEmailAddress = AdminEmail,
            AdminPassword = AdminPassword
        });

        harness.Steps.ShouldBe(new[] { "migrate", "seed" });
        await harness.Migrator.Received(1).MigrateSchemaAsync(tenant.Id);
    }

    [Fact]
    public async Task Shared_database_and_database_name_without_a_connection_string_do_not_migrate()
    {
        var shared = new TestTenant(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "shared");
        var sharedHarness = new Harness(shared);
        await sharedHarness.Service.CreateAsync(Input("shared"));
        sharedHarness.Steps.ShouldBe(new[] { "seed" });
        await sharedHarness.Migrator.DidNotReceive().MigrateSchemaAsync(Arg.Any<Guid>());

        var named = new TestTenant(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "named");
        named.SetDatabaseName("named_acme");
        var namedHarness = new Harness(named);
        await namedHarness.Service.CreateAsync(Input("named"));
        namedHarness.Steps.ShouldBe(new[] { "seed" });
        await namedHarness.Migrator.DidNotReceive().MigrateSchemaAsync(Arg.Any<Guid>());
    }

    private static TenantCreateDto Input(string name)
    {
        return new TenantCreateDto
        {
            Name = name,
            AdminEmailAddress = AdminEmail,
            AdminPassword = AdminPassword
        };
    }

    private sealed class CreatedAdmin
    {
        public IdentityUser User { get; set; }
        public string Hash { get; set; }
        public PasswordHasher<IdentityUser> Hasher { get; } = new();
    }

    private sealed class Harness
    {
        public TenantAppService Service { get; }
        public IDistributedEventBus Bus { get; }
        public ITenantSchemaMigrator Migrator { get; }
        public RecordingDataSeeder Seeder { get; }
        public List<string> Steps { get; } = new();

        public Harness(Tenant tenant)
        {
            var tenants = Substitute.For<ITenantRepository>();
            tenants.InsertAsync(Arg.Any<Tenant>(), true, Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<Tenant>());
            tenants.FindAsync(tenant.Id, true, Arg.Any<CancellationToken>()).Returns(tenant);

            var manager = Substitute.For<ITenantManager>();
            manager.CreateAsync(tenant.Name).Returns(tenant);

            Migrator = Substitute.For<ITenantSchemaMigrator>();
            Migrator.MigrateSchemaAsync(tenant.Id).Returns(_ =>
            {
                Steps.Add("migrate");
                return Task.CompletedTask;
            });

            var identity = Substitute.For<IIdentityDataSeeder>();
            var current = new MutableCurrentTenant();
            Seeder = new RecordingDataSeeder(new IdentityDataSeedContributor(identity), current, Steps);
            Bus = Substitute.For<IDistributedEventBus>();
            Bus.PublishAsync(Arg.Any<TenantCreatedEto>(), true, true).Returns(Task.CompletedTask);
            var dedicated = new TenantDedicatedSchemaMigrator(tenants, new[] { Migrator });

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
            unitOfWorkManager.Current.Returns(unitOfWork);

            var authorization = Substitute.For<IAbpAuthorizationService>();
            authorization.AuthorizeAsync(
                    Arg.Any<System.Security.Claims.ClaimsPrincipal>(),
                    Arg.Any<object>(),
                    Arg.Any<string>())
                .Returns(AuthorizationResult.Success());

            var services = new ServiceCollection();
            services.AddSingleton<ICurrentTenant>(current);
            services.AddSingleton(unitOfWorkManager);
            services.AddSingleton<IAuthorizationService>(authorization);
            Service = new TenantAppService(tenants, manager, Seeder, Bus, dedicated)
            {
                LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider())
            };

            Created = new CreatedAdmin();
            identity.SeedAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Guid?>(),
                    Arg.Any<string>())
                .Returns(call =>
                {
                    var email = call.ArgAt<string>(0);
                    var password = call.ArgAt<string>(1);
                    var tenantId = call.ArgAt<Guid?>(2);
                    var userName = call.ArgAt<string>(3);
                    Created.User = new IdentityUser(Guid.NewGuid(), userName, email, tenantId);
                    Created.Hash = Created.Hasher.HashPassword(Created.User, password);
                    return new IdentityDataSeedResult { CreatedAdminUser = true };
                });
        }

        public CreatedAdmin Created { get; }

        public CreatedAdmin CaptureAdmin() => Created;
    }

    private sealed class RecordingDataSeeder : IDataSeeder
    {
        private readonly IdentityDataSeedContributor _contributor;
        private readonly MutableCurrentTenant _current;
        private readonly List<string> _steps;

        public RecordingDataSeeder(
            IdentityDataSeedContributor contributor,
            MutableCurrentTenant current,
            List<string> steps)
        {
            _contributor = contributor;
            _current = current;
            _steps = steps;
        }

        public Guid? SeenTenantId { get; private set; }

        public DataSeedContext Context { get; private set; }

        public Task SeedAsync(DataSeedContext context)
        {
            _steps.Add("seed");
            SeenTenantId = _current.Id;
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
