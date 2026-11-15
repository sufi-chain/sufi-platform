using Shouldly;
using SufiChain.SufiPlatform.SufiSaas.Licensing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public sealed class IssuedLicenseRepositoryRoundTripTests : SufiSaasTestBase
{
    [Fact]
    public async Task Repository_round_trips_a_host_licence_with_its_tenant_and_lease()
    {
        var id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var tenantId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var requestId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var confirmedAt = new DateTime(2026, 11, 15, 8, 0, 0, DateTimeKind.Utc);
        var revokedAt = new DateTime(2026, 11, 16, 9, 30, 0, DateTimeKind.Utc);
        var license = new IssuedLicense(id, id.ToString("N"), "signed-token", tenantId, requestId, "instance-1");
        license.Overwrite(
            "signed-token",
            tenantId,
            requestId,
            "instance-1",
            "replacement-token",
            12,
            4096,
            "instance-1",
            "0.1.0",
            confirmedAt,
            revokedAt,
            "signed-lease");

        var repository = GetRequiredService<IIssuedLicenseRepository>();
        using (var write = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false))
        {
            await repository.InsertAsync(license, autoSave: true);
            await write.CompleteAsync();
        }

        using var read = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        var loaded = await repository.FindByLicenseIdAsync(id.ToString("N"));
        loaded.ShouldNotBeNull();
        loaded.Id.ShouldBe(id);
        loaded.TenantId.ShouldBe(tenantId);
        loaded.LicenseId.ShouldBe(id.ToString("N"));
        loaded.Token.ShouldBe("signed-token");
        loaded.TenantRequestId.ShouldBe(requestId);
        loaded.InstanceId.ShouldBe("instance-1");
        loaded.ReplacementToken.ShouldBe("replacement-token");
        loaded.LastUserCount.ShouldBe(12);
        loaded.LastDatabaseBytes.ShouldBe(4096);
        loaded.LastInstanceId.ShouldBe("instance-1");
        loaded.LastPlatformVersion.ShouldBe("0.1.0");
        loaded.LastLease.ShouldBe("signed-lease");
        AsUtc(loaded.LastConfirmedAt).ShouldBe(confirmedAt);
        AsUtc(loaded.RevokedAt).ShouldBe(revokedAt);
        (await repository.FindByTenantRequestIdAsync(requestId))!.Id.ShouldBe(id);
        (await repository.FindByLicenseIdAsync("../x")).ShouldBeNull();
        await read.CompleteAsync();
    }

    private static DateTime? AsUtc(DateTime? value) =>
        value is null ? null : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
}
