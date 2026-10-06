using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Licensing;
using SufiChain.SufiPlatform.SufiSaas.Licensing;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public sealed class LicenseConfirmPathTraversalTests : IDisposable
{
    private const string SecretToken = "secret-token";

    private readonly string _root;
    private readonly string _store;
    private readonly string _outside;
    private readonly SelfHostedLicenseIssuer _issuer;
    private readonly LicenseAppService _service;

    public LicenseConfirmPathTraversalTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "sufi-license-confirm-" + Guid.NewGuid().ToString("N"));
        _store = Path.Combine(_root, "issued");
        _outside = Path.Combine(_root, "outside");
        Directory.CreateDirectory(_store);
        Directory.CreateDirectory(_outside);
        File.WriteAllText(Path.Combine(_root, "x.json"), SecretDocument("root-x"));
        File.WriteAllText(Path.Combine(_outside, "secret.json"), SecretDocument("outside-secret"));
        _issuer = NewIssuer(_store);
        _service = NewService(_issuer);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    public static TheoryData<string> RejectedLicenseIds { get; } = new()
    {
        "../x",
        "..\\x",
        "..\\\\x",
        "%2e%2e%2fx",
        "%2e%2e%5cx",
        "..%2f..%2fx",
        "%252e%252e%252fx",
        "",
        new string('f', 33),
        new string('a', 4096),
        "C:\\Windows\\Temp\\license"
    };

    [Theory]
    [MemberData(nameof(RejectedLicenseIds))]
    public async Task Confirm_Should_Reject_An_Unsafe_License_Id_Without_File_Access(string licenseId)
    {
        var before = Snapshot(_root);

        var found = await _issuer.FindByLicenseIdAsync(licenseId);
        found.ShouldBeNull();

        var response = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            UserCount = 9
        });
        response.Accepted.ShouldBeFalse();
        response.ReplacementToken.ShouldBeNull();
        AssertDoesNotEcho(response.Message, licenseId);

        var error = await Should.ThrowAsync<BusinessException>(() => _issuer.SaveAsync(Hostile(licenseId)));
        error.Code.ShouldBe(SufiSaasErrorCodes.LicenseNotFound);
        AssertDoesNotEcho(error.Message, licenseId);
        AssertDoesNotEcho(error.Details, licenseId);
        Snapshot(_root).ShouldBe(before);
    }

    [Fact]
    public async Task Confirm_Should_Not_Read_Or_Write_An_Absolute_Path()
    {
        var bait = Path.Combine(_outside, "secret.json");
        var before = File.ReadAllText(bait);
        var licenseId = bait[..^".json".Length];

        var found = await _issuer.FindByLicenseIdAsync(licenseId);
        found.ShouldBeNull();

        var response = await _service.ConfirmAsync(new ConfirmLicenseRequest { LicenseId = licenseId });
        response.Accepted.ShouldBeFalse();
        AssertDoesNotEcho(response.Message, licenseId);

        var error = await Should.ThrowAsync<BusinessException>(() => _issuer.SaveAsync(Hostile(licenseId)));
        error.Code.ShouldBe(SufiSaasErrorCodes.LicenseNotFound);
        AssertDoesNotEcho(error.Message, licenseId);
        File.ReadAllText(bait).ShouldBe(before);
        File.Exists(bait + ".json").ShouldBeFalse();
    }

    [Fact]
    public async Task Confirm_Should_Read_And_Write_A_Canonical_License_Id()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        SelfHostedLicenseIssuer.IsCanonicalLicenseId(licenseId).ShouldBeTrue();

        await _issuer.SaveAsync(new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "issued-token"
        });

        var loaded = await _issuer.FindByLicenseIdAsync(licenseId);
        loaded.ShouldNotBeNull();
        loaded.Token.ShouldBe("issued-token");
        loaded.LicenseId.ShouldBe(licenseId);

        var storedPath = Path.Combine(_store, licenseId + ".json");
        File.Exists(storedPath).ShouldBeTrue();
        Path.GetDirectoryName(storedPath).ShouldBe(_store);

        var response = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            UserCount = 4,
            DatabaseBytes = 128,
            InstanceId = "instance-1",
            PlatformVersion = "0.1.0"
        });

        response.Accepted.ShouldBeTrue();
        response.Message.ShouldBeNull();

        var confirmed = await _issuer.FindByLicenseIdAsync(licenseId);
        confirmed.ShouldNotBeNull();
        confirmed.Token.ShouldBe("issued-token");
        confirmed.InstanceId.ShouldBe("instance-1");
        confirmed.LastUserCount.ShouldBe(4);
        confirmed.LastDatabaseBytes.ShouldBe(128);
        confirmed.LastInstanceId.ShouldBe("instance-1");
        confirmed.LastPlatformVersion.ShouldBe("0.1.0");
        confirmed.LastConfirmedAt.ShouldBe(new DateTimeOffset(new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc)));
        Directory.GetFiles(_store, "*.json").Length.ShouldBe(1);
        Directory.GetFiles(_outside, "*.json").Select(Path.GetFileName).ShouldBe(new[] { "secret.json" });
    }

    [Fact]
    public async Task Confirm_Should_Reject_An_Unknown_Canonical_License_Id()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        var before = Snapshot(_root);

        var response = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            InstanceId = "instance-1",
            UserCount = 3
        });

        response.Accepted.ShouldBeFalse();
        response.ReplacementToken.ShouldBeNull();
        response.Message.ShouldBeNull();
        Snapshot(_root).ShouldBe(before);
    }

    [Fact]
    public async Task Confirm_Should_Reject_A_Revoked_License_Without_Updating_Usage()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        var revokedAt = new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        await _issuer.SaveAsync(new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "issued-token",
            InstanceId = "instance-1",
            RevokedAt = revokedAt
        });

        var response = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            InstanceId = "instance-1",
            UserCount = 9,
            DatabaseBytes = 64
        });

        response.Accepted.ShouldBeFalse();
        response.Message.ShouldBeNull();
        var stored = await _issuer.FindByLicenseIdAsync(licenseId);
        stored.ShouldNotBeNull();
        stored.RevokedAt.ShouldBe(revokedAt);
        stored.LastUserCount.ShouldBe(0);
        stored.LastDatabaseBytes.ShouldBe(0);
        stored.LastConfirmedAt.ShouldBeNull();
        stored.Token.ShouldBe("issued-token");
    }

    [Fact]
    public async Task Confirm_Should_Bind_The_First_Instance_And_Reject_Another()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        await _issuer.SaveAsync(new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "issued-token"
        });

        var first = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            InstanceId = "  instance-1  ",
            UserCount = 2
        });
        first.Accepted.ShouldBeTrue();

        var other = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            InstanceId = "instance-2",
            UserCount = 99,
            DatabaseBytes = 500
        });
        other.Accepted.ShouldBeFalse();
        other.ReplacementToken.ShouldBeNull();

        var bound = await _issuer.FindByLicenseIdAsync(licenseId);
        bound.ShouldNotBeNull();
        bound.InstanceId.ShouldBe("instance-1");
        bound.LastInstanceId.ShouldBe("instance-1");
        bound.LastUserCount.ShouldBe(2);
        bound.LastDatabaseBytes.ShouldBe(0);

        var same = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            InstanceId = "instance-1",
            UserCount = 6,
            DatabaseBytes = 32,
            PlatformVersion = "0.2.0"
        });
        same.Accepted.ShouldBeTrue();

        var updated = await _issuer.FindByLicenseIdAsync(licenseId);
        updated.ShouldNotBeNull();
        updated.InstanceId.ShouldBe("instance-1");
        updated.LastUserCount.ShouldBe(6);
        updated.LastDatabaseBytes.ShouldBe(32);
        updated.LastPlatformVersion.ShouldBe("0.2.0");
        updated.Token.ShouldBe("issued-token");
    }

    [Fact]
    public async Task Confirm_Should_Reject_A_Confirm_That_Does_Not_Name_An_Instance()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        await _issuer.SaveAsync(new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "issued-token"
        });
        var before = await File.ReadAllTextAsync(Path.Combine(_store, licenseId + ".json"));

        var response = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            UserCount = 4
        });

        response.Accepted.ShouldBeFalse();
        (await File.ReadAllTextAsync(Path.Combine(_store, licenseId + ".json"))).ShouldBe(before);
    }

    [Fact]
    public async Task Confirm_Should_Reject_A_Confirm_That_Does_Not_Match_The_Issued_Instance()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        await _issuer.SaveAsync(new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "issued-token",
            InstanceId = "issued-a"
        });

        var response = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            InstanceId = "other-install",
            UserCount = 8
        });

        response.Accepted.ShouldBeFalse();
        var stored = await _issuer.FindByLicenseIdAsync(licenseId);
        stored.ShouldNotBeNull();
        stored.InstanceId.ShouldBe("issued-a");
        stored.LastInstanceId.ShouldBeNull();
        stored.LastUserCount.ShouldBe(0);
        stored.LastConfirmedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Revoke_Should_Stop_Later_Confirms_For_That_Request()
    {
        var licenseId = Guid.NewGuid().ToString("N");
        var tenantRequestId = Guid.NewGuid();
        await _issuer.SaveAsync(new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "issued-token",
            TenantRequestId = tenantRequestId,
            InstanceId = "instance-1"
        });

        await _issuer.RevokeByTenantRequestIdAsync(
            tenantRequestId,
            new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc));

        var response = await _service.ConfirmAsync(new ConfirmLicenseRequest
        {
            LicenseId = licenseId,
            InstanceId = "instance-1",
            UserCount = 1
        });

        response.Accepted.ShouldBeFalse();
        var stored = await _issuer.FindByLicenseIdAsync(licenseId);
        stored.ShouldNotBeNull();
        stored.RevokedAt.ShouldBe(new DateTimeOffset(new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc)));
        stored.LastConfirmedAt.ShouldBeNull();
    }

    private static SelfHostedLicenseIssuer NewIssuer(string directory)
    {
        return new SelfHostedLicenseIssuer(
            null!,
            Options.Create(new SufiSaasLicensingOptions
            {
                IssuedLicenseDirectory = directory
            }),
            null!,
            Substitute.For<IClock>(),
            SimpleGuidGenerator.Instance);
    }

    private static LicenseAppService NewService(SelfHostedLicenseIssuer issuer)
    {
        IClock clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc));
        var services = new ServiceCollection();
        services.AddSingleton(clock);
        var service = new LicenseAppService(issuer);
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private static IssuedLicenseRecord Hostile(string licenseId)
    {
        return new IssuedLicenseRecord
        {
            LicenseId = licenseId,
            Token = "overwrite"
        };
    }

    private static string SecretDocument(string marker)
    {
        return JsonSerializer.Serialize(new IssuedLicenseRecord
        {
            LicenseId = marker,
            Token = SecretToken
        });
    }

    private static string Snapshot(string root)
    {
        return string.Join(
            "\n",
            Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => path + "=" + File.ReadAllText(path)));
    }

    private static void AssertDoesNotEcho(string? text, string licenseId)
    {
        if (licenseId.Length == 0 || string.IsNullOrEmpty(text))
        {
            return;
        }

        text.ShouldNotContain(licenseId);
    }
}
