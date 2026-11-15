using System.Security.Cryptography;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Licensing;

public sealed class LicenseLeaseTests : IDisposable
{
    private readonly RSA _key = RSA.Create(2048);
    private readonly LicenseLeaseService _leases = new();
    private readonly LicenseTokenService _tokens = new();

    public void Dispose() => _key.Dispose();

    [Fact]
    public void Valid_lease_verifies_offline_with_the_public_key()
    {
        var confirmed = new DateTimeOffset(2026, 11, 15, 8, 0, 0, TimeSpan.Zero);
        var signed = _leases.Sign(new LicenseLease
        {
            LicenseId = "abc",
            InstanceId = "instance-a",
            ConfirmedAt = confirmed,
            ExpiresAt = confirmed.AddDays(7)
        }, _key.ExportRSAPrivateKeyPem());

        var lease = _leases.Validate(signed, _key.ExportSubjectPublicKeyInfoPem(), "instance-a");

        lease.LicenseId.ShouldBe("abc");
        lease.InstanceId.ShouldBe("instance-a");
        lease.ConfirmedAt.ShouldBe(confirmed);
        lease.ExpiresAt.ToUnixTimeSeconds().ShouldBe(confirmed.AddDays(7).ToUnixTimeSeconds());
    }

    [Fact]
    public void Tampered_lease_is_rejected()
    {
        var signed = SignValid();
        var pieces = signed.Split('.');
        var signature = pieces[2].ToCharArray();
        signature[0] = signature[0] == 'A' ? 'B' : 'A';
        pieces[2] = new string(signature);
        var tampered = string.Join('.', pieces);

        var error = Should.Throw<BusinessException>(() =>
            _leases.Validate(tampered, _key.ExportSubjectPublicKeyInfoPem(), "instance-a"));

        error.Code.ShouldBe(LicenseErrorCodes.InvalidToken);
    }

    [Fact]
    public void Expired_lease_is_rejected()
    {
        var confirmed = DateTimeOffset.UtcNow.AddDays(-10);
        var signed = _leases.Sign(new LicenseLease
        {
            LicenseId = "abc",
            InstanceId = "instance-a",
            ConfirmedAt = confirmed,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        }, _key.ExportRSAPrivateKeyPem());

        var error = Should.Throw<BusinessException>(() =>
            _leases.Validate(signed, _key.ExportSubjectPublicKeyInfoPem(), "instance-a"));

        error.Code.ShouldBe(LicenseErrorCodes.InvalidToken);
    }

    [Fact]
    public void A_licence_token_is_not_a_lease()
    {
        var token = _tokens.Sign(new LicenseEntitlement
        {
            LicenseId = "abc",
            Products = new[] { LicenseProducts.SufiCom },
            InstanceId = "instance-a",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        }, _key.ExportRSAPrivateKeyPem());

        Should.Throw<BusinessException>(() =>
            _leases.Validate(token, _key.ExportSubjectPublicKeyInfoPem(), "instance-a"));
    }

    private string SignValid()
    {
        var confirmed = DateTimeOffset.UtcNow;
        return _leases.Sign(new LicenseLease
        {
            LicenseId = "abc",
            InstanceId = "instance-a",
            ConfirmedAt = confirmed,
            ExpiresAt = confirmed.AddDays(7)
        }, _key.ExportRSAPrivateKeyPem());
    }
}
