using System.Threading;
using NSubstitute;
using Shouldly;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.Permissions;

public class PermissionDataSeederTests
{
    [Fact]
    public async Task SeedAsync_inserts_each_grant_once()
    {
        var tenantId = Guid.Parse("3d5e3f4e-bf3e-6920-e97f-3a240c884ab0");
        var tenant = new MutableTenant();
        var store = new List<PermissionGrant>
        {
            new(Guid.NewGuid(), "SufiDashboard.Settings", "R", "admin", tenantId)
        };
        var repository = Substitute.For<IPermissionGrantRepository>();
        repository
            .FindAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var found = store.FirstOrDefault(grant =>
                    grant.Name == call.ArgAt<string>(0) &&
                    grant.ProviderName == call.ArgAt<string>(1) &&
                    grant.ProviderKey == call.ArgAt<string>(2) &&
                    grant.TenantId == tenant.Id);
                return Task.FromResult<PermissionGrant>(found!);
            });
        repository
            .InsertAsync(Arg.Any<PermissionGrant>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var grant = call.Arg<PermissionGrant>();
                store.Add(grant);
                return Task.FromResult(grant);
            });

        var seeder = new PermissionDataSeeder(repository, SimpleGuidGenerator.Instance, tenant);
        var names = new[] { "SufiDashboard.Settings", "SufiLocalization.Settings" };

        await seeder.SeedAsync("R", "admin", names, tenantId);
        await seeder.SeedAsync("R", "admin", names, tenantId);

        store.Count(grant => grant.Name == "SufiDashboard.Settings" && grant.TenantId == tenantId).ShouldBe(1);
        store.Count(grant => grant.Name == "SufiLocalization.Settings" && grant.TenantId == tenantId).ShouldBe(1);
        store.Count.ShouldBe(2);
    }

    private sealed class MutableTenant : ICurrentTenant
    {
        public Guid? Id { get; private set; }

        public string? Name { get; private set; }

        public bool IsAvailable => Id.HasValue;

        public IDisposable Change(Guid? id, string? name = null)
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

            public Restore(Action restore) => _restore = restore;

            public void Dispose() => _restore();
        }
    }
}
