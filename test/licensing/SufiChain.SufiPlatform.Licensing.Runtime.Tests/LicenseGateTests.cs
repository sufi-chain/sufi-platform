using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Licensing;

public sealed class LicenseGateTests : IDisposable
{
    private const string InstanceId = "instance-a";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "sufi-license-tests", Guid.NewGuid().ToString("N"));
    private readonly RSA _signingKey = RSA.Create(2048);
    private readonly LicenseTokenService _tokens = new();

    private string LicensePath => Path.Combine(_directory, "license.json");

    public void Dispose()
    {
        _signingKey.Dispose();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public async Task Missing_license_file_fails_closed()
    {
        var gate = CreateGate();

        var entitlement = await gate.GetEntitlementAsync();

        entitlement.IsPresent.ShouldBeFalse();
        entitlement.BannerReason.ShouldBe("LicenseBanner:ReadOnly");
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();
    }

    [Fact]
    public async Task EnsureCanWrite_throws_write_denied_with_product_when_denied()
    {
        var gate = CreateGate();

        var exception = await Should.ThrowAsync<BusinessException>(
            () => gate.EnsureCanWriteAsync(LicenseProducts.SufiComChat));

        exception.Code.ShouldBe(LicenseErrorCodes.WriteDenied);
        exception.Data["Product"].ShouldBe(LicenseProducts.SufiComChat);
    }

    [Fact]
    public async Task Issuer_host_short_circuits_to_all_products()
    {
        var gate = CreateGate(issuer: new IssuerHost(true));

        var entitlement = await gate.GetEntitlementAsync();

        entitlement.Tier.ShouldBe("Issuer");
        foreach (var product in LicenseProducts.All)
        {
            (await gate.CanWriteAsync(product)).ShouldBeTrue();
        }
    }

    [Fact]
    public void Default_issuer_host_is_not_an_issuer()
    {
        new NullLicenseIssuerHost().IsIssuer.ShouldBeFalse();
    }

    [Fact]
    public async Task Valid_token_allows_only_licensed_products()
    {
        WriteToken(Sign(new LicenseEntitlement
        {
            LicenseId = "lic-1",
            Tier = LicenseTiers.FreeSelfHosted,
            Products = new[] { LicenseProducts.SufiCom },
            InstanceId = InstanceId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        }));
        var gate = CreateGate();

        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeTrue();
        (await gate.CanWriteAsync(LicenseProducts.SufiFinance)).ShouldBeFalse();
        (await gate.CanWriteAsync(string.Empty)).ShouldBeFalse();
    }

    [Fact]
    public async Task Token_signed_by_another_key_is_rejected()
    {
        using var otherKey = RSA.Create(2048);
        WriteToken(_tokens.Sign(ValidEntitlement(), otherKey.ExportRSAPrivateKeyPem()));
        var gate = CreateGate();

        var entitlement = await gate.GetEntitlementAsync();

        entitlement.IsPresent.ShouldBeTrue();
        entitlement.SignatureValid.ShouldBeFalse();
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();
    }

    [Fact]
    public async Task Token_bound_to_another_instance_is_rejected()
    {
        var entitlement = ValidEntitlement();
        WriteToken(Sign(new LicenseEntitlement
        {
            LicenseId = entitlement.LicenseId,
            Products = entitlement.Products,
            InstanceId = "instance-b",
            ExpiresAt = entitlement.ExpiresAt
        }));
        var gate = CreateGate();

        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();
    }

    [Fact]
    public async Task Missing_public_key_is_rejected()
    {
        WriteToken(Sign(ValidEntitlement()));
        var gate = CreateGate(withPublicKey: false);

        (await gate.GetEntitlementAsync()).SignatureValid.ShouldBeFalse();
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();
    }

    [Fact]
    public void Empty_token_is_rejected_by_token_service()
    {
        var exception = Should.Throw<BusinessException>(
            () => _tokens.Validate(string.Empty, _signingKey.ExportSubjectPublicKeyInfoPem(), InstanceId));

        exception.Code.ShouldBe(LicenseErrorCodes.InvalidToken);
    }

    [Fact]
    public async Task Usage_over_licensed_user_limit_denies_writes()
    {
        WriteToken(Sign(new LicenseEntitlement
        {
            LicenseId = "lic-1",
            Products = new[] { LicenseProducts.SufiCom },
            InstanceId = InstanceId,
            MaxUsers = 5,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        }));
        var gate = CreateGate(reporter: new FixedReporter(new LicenseHeartbeatSnapshot { UserCount = 6 }));

        (await gate.GetEntitlementAsync()).BannerReason.ShouldBe("LicenseBanner:OverLimit");
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();
    }

    [Fact]
    public async Task Confirmation_older_than_offline_grace_denies_writes()
    {
        WriteToken(Sign(ValidEntitlement()));
        var gate = CreateGate();
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeTrue();

        gate.RememberConfirm(DateTimeOffset.UtcNow.AddDays(-8));

        (await gate.GetEntitlementAsync()).BannerReason.ShouldBe("LicenseBanner:ReadOnly");
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();
    }

    [Fact]
    public async Task Valid_lease_verifies_offline_and_anchors_grace_to_lease_expiry()
    {
        var confirmed = DateTimeOffset.UtcNow.AddDays(-10);
        var lease = new LicenseLeaseService().Sign(new LicenseLease
        {
            LicenseId = "lic-1",
            InstanceId = InstanceId,
            ConfirmedAt = confirmed,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        }, _signingKey.ExportRSAPrivateKeyPem());
        WriteDocument(Sign(ValidEntitlement()), lease);

        var gate = CreateGate();

        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeTrue();
        (await gate.GetEntitlementAsync()).BannerReason.ShouldBeNull();
    }

    [Fact]
    public async Task Tampered_lease_denies_writes_without_a_live_call()
    {
        var lease = new LicenseLeaseService().Sign(new LicenseLease
        {
            LicenseId = "lic-1",
            InstanceId = InstanceId,
            ConfirmedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        }, _signingKey.ExportRSAPrivateKeyPem());
        var pieces = lease.Split('.');
        var signature = pieces[2].ToCharArray();
        signature[0] = signature[0] == 'A' ? 'B' : 'A';
        pieces[2] = new string(signature);
        WriteDocument(Sign(ValidEntitlement()), string.Join('.', pieces));

        (await CreateGate().CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();
    }

    [Fact]
    public async Task Expired_lease_denies_writes()
    {
        var lease = new LicenseLeaseService().Sign(new LicenseLease
        {
            LicenseId = "lic-1",
            InstanceId = InstanceId,
            ConfirmedAt = DateTimeOffset.UtcNow.AddDays(-10),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        }, _signingKey.ExportRSAPrivateKeyPem());
        WriteDocument(Sign(ValidEntitlement()), lease);

        var entitlement = await CreateGate().GetEntitlementAsync();

        entitlement.SignatureValid.ShouldBeTrue();
        entitlement.BannerReason.ShouldBe("LicenseBanner:ReadOnly");
    }

    [Fact]
    public async Task ReplaceToken_writes_the_file_and_reloads()
    {
        var gate = CreateGate();
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeFalse();

        gate.ReplaceToken(Sign(ValidEntitlement()));

        File.Exists(LicensePath).ShouldBeTrue();
        (await gate.CanWriteAsync(LicenseProducts.SufiCom)).ShouldBeTrue();
    }

    private LicenseGate CreateGate(
        ILicenseIssuerHost? issuer = null,
        ILicenseHeartbeatReporter? reporter = null,
        bool withPublicKey = true)
    {
        var options = new SufiLicensingOptions
        {
            LicensePath = LicensePath,
            InstanceId = InstanceId,
            PublicKeyPem = withPublicKey ? _signingKey.ExportSubjectPublicKeyInfoPem() : null
        };

        return new LicenseGate(
            _tokens,
            reporter ?? new FixedReporter(new LicenseHeartbeatSnapshot()),
            issuer ?? new NullLicenseIssuerHost(),
            Options.Create(options));
    }

    private static LicenseEntitlement ValidEntitlement() => new()
    {
        LicenseId = "lic-1",
        Products = new[] { LicenseProducts.SufiCom },
        InstanceId = InstanceId,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
    };

    private string Sign(LicenseEntitlement entitlement)
        => _tokens.Sign(entitlement, _signingKey.ExportRSAPrivateKeyPem());

    private void WriteToken(string token) => WriteDocument(token, null);

    private void WriteDocument(string token, string? lease)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(LicensePath, JsonSerializer.Serialize(new LicenseFileDocument
        {
            Token = token,
            Lease = lease
        }));
    }

    private sealed class IssuerHost(bool isIssuer) : ILicenseIssuerHost
    {
        public bool IsIssuer { get; } = isIssuer;
    }

    private sealed class FixedReporter(LicenseHeartbeatSnapshot snapshot) : ILicenseHeartbeatReporter
    {
        public Task<LicenseHeartbeatSnapshot> CaptureAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(snapshot);
    }
}
