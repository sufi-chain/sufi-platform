using System.Text.Json;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.SufiSaas.Licensing;
using Volo.Abp.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public sealed class IssuedLicenseJsonImportTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "sufi-license-import-" + Guid.NewGuid().ToString("N"));

    public IssuedLicenseJsonImportTests()
    {
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task Import_is_idempotent_and_does_not_overwrite_the_database_row()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        var otherId = Guid.NewGuid().ToString("N");
        Write(licenseId, new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "imported-token",
            TenantRequestId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            InstanceId = "instance-a",
            LastUserCount = 3
        });
        Write(otherId, new IssuedLicenseRecord
        {
            LicenseId = otherId,
            Token = "second-token"
        });
        Write("not-a-guid", new IssuedLicenseRecord { LicenseId = "not-a-guid", Token = "skip" });
        var mismatched = Guid.NewGuid().ToString("N");
        Write(mismatched, new IssuedLicenseRecord { LicenseId = Guid.NewGuid().ToString("N"), Token = "mismatch" });

        var repository = IssuedLicenseRepositoryDouble.Create();
        var importer = NewImporter(repository);

        var first = await importer.ImportAsync();
        first.Inserted.ShouldBe(2);
        first.Skipped.ShouldBe(2);

        Write(licenseId, new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "changed-after-import"
        });

        var second = await importer.ImportAsync();
        second.Inserted.ShouldBe(0);
        second.Skipped.ShouldBe(4);

        var stored = await repository.FindByLicenseIdAsync(licenseId);
        stored.ShouldNotBeNull();
        stored.Token.ShouldBe("imported-token");
        stored.TenantId.ShouldBeNull();
        stored.TenantRequestId.ShouldBe(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        stored.InstanceId.ShouldBe("instance-a");
        stored.LastUserCount.ShouldBe(3);
        stored.Id.ShouldBe(Guid.ParseExact(licenseId, "N"));
        (await repository.FindByLicenseIdAsync(otherId)).ShouldNotBeNull();
        (await repository.FindByLicenseIdAsync(mismatched)).ShouldBeNull();
    }

    [Fact]
    public async Task Tenant_seed_does_not_import()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        Write(licenseId, new IssuedLicenseRecord { LicenseId = licenseId, Token = "tenant-skip" });
        var repository = IssuedLicenseRepositoryDouble.Create();
        var contributor = new IssuedLicenseJsonImportDataSeedContributor(NewImporter(repository));

        await contributor.SeedAsync(new DataSeedContext(Guid.NewGuid()));

        (await repository.FindByLicenseIdAsync(licenseId)).ShouldBeNull();

        await contributor.SeedAsync(new DataSeedContext(null));
        (await repository.FindByLicenseIdAsync(licenseId)).ShouldNotBeNull();
        await contributor.SeedAsync(new DataSeedContext(null));
        (await repository.FindByLicenseIdAsync(licenseId))!.Token.ShouldBe("tenant-skip");
    }

    private IssuedLicenseJsonImporter NewImporter(IIssuedLicenseRepository repository)
    {
        return new IssuedLicenseJsonImporter(
            repository,
            Options.Create(new SufiSaasLicensingOptions { IssuedLicenseDirectory = _root }));
    }

    private void Write(string fileName, IssuedLicenseRecord record)
    {
        File.WriteAllText(Path.Combine(_root, fileName + ".json"), JsonSerializer.Serialize(record));
    }
}
