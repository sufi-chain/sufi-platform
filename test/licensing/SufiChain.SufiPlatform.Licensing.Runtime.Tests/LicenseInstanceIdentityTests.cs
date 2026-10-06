using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Licensing;

public sealed class LicenseInstanceIdentityTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "sufi-instance-id-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Ensure_persists_one_id_beside_the_licence_file()
    {
        var options = new SufiLicensingOptions
        {
            LicensePath = Path.Combine(_directory, "ce-license.json")
        };

        var first = LicenseInstanceIdentity.Ensure(options);
        var second = LicenseInstanceIdentity.Ensure(new SufiLicensingOptions
        {
            LicensePath = options.LicensePath
        });

        Guid.TryParseExact(first, "N", out _).ShouldBeTrue();
        second.ShouldBe(first);
        File.ReadAllText(Path.Combine(_directory, LicenseInstanceIdentity.FileName)).Trim().ShouldBe(first);
    }

    [Fact]
    public void Ensure_keeps_a_configured_instance_id()
    {
        var options = new SufiLicensingOptions
        {
            LicensePath = Path.Combine(_directory, "ce-license.json"),
            InstanceId = "  instance-1  "
        };

        LicenseInstanceIdentity.Ensure(options).ShouldBe("instance-1");
        options.InstanceId.ShouldBe("instance-1");
        File.Exists(Path.Combine(_directory, LicenseInstanceIdentity.FileName)).ShouldBeFalse();
    }
}
