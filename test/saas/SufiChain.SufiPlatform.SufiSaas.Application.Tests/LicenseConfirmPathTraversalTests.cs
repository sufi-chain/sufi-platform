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
        confirmed.LastUserCount.ShouldBe(4);
        confirmed.LastDatabaseBytes.ShouldBe(128);
        confirmed.LastInstanceId.ShouldBe("instance-1");
        confirmed.LastPlatformVersion.ShouldBe("0.1.0");
        confirmed.LastConfirmedAt.ShouldBe(new DateTimeOffset(new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc)));
        Directory.GetFiles(_store, "*.json").Length.ShouldBe(1);
        Directory.GetFiles(_outside, "*.json").Select(Path.GetFileName).ShouldBe(new[] { "secret.json" });
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
