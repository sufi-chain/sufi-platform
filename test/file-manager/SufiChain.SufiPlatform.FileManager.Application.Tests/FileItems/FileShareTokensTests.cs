using System;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.FileItems;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

/// <summary>Quick Share tokens (sufi-chain#436): create, resolve, expired, revoked, cross-tenant.</summary>
public class FileShareTokensTests
{
    private const string Secret = "test-secret-for-quick-share";
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);

    private static FileShareTokens.Payload NewPayload(Guid? tenantId = null, int minutes = 60, int version = 0) =>
        new(Guid.NewGuid(), tenantId, Now.AddMinutes(minutes), version);

    [Fact]
    public void Created_token_resolves_to_the_same_file_and_tenant()
    {
        var tenant = Guid.NewGuid();
        var payload = NewPayload(tenant);
        var token = FileShareTokens.Create(Secret, payload);

        token.ShouldNotContain(payload.FileId.ToString());
        token.ShouldNotContain(payload.FileId.ToString("N"));
        FileShareTokens.Validate(Secret, token, Now, out var resolved).ShouldBe(FileShareLinkStatus.Valid);
        resolved.FileId.ShouldBe(payload.FileId);
        resolved.TenantId.ShouldBe(tenant);
        FileShareTokens.CheckAgainstFile(resolved, tenant, 0).ShouldBe(FileShareLinkStatus.Valid);
    }

    [Fact]
    public void Token_stops_working_after_its_expiry()
    {
        var token = FileShareTokens.Create(Secret, NewPayload(minutes: 5));

        FileShareTokens.Validate(Secret, token, Now.AddMinutes(4), out _).ShouldBe(FileShareLinkStatus.Valid);
        FileShareTokens.Validate(Secret, token, Now.AddMinutes(5), out _).ShouldBe(FileShareLinkStatus.Expired);
    }

    [Fact]
    public void Revoking_bumps_the_version_so_older_links_fail()
    {
        var token = FileShareTokens.Create(Secret, NewPayload(version: 2));
        FileShareTokens.Validate(Secret, token, Now, out var resolved).ShouldBe(FileShareLinkStatus.Valid);

        FileShareTokens.CheckAgainstFile(resolved, null, currentVersion: 3).ShouldBe(FileShareLinkStatus.Revoked);
    }

    [Fact]
    public void Token_from_another_tenant_does_not_open_the_file()
    {
        var token = FileShareTokens.Create(Secret, NewPayload(Guid.NewGuid()));
        FileShareTokens.Validate(Secret, token, Now, out var resolved).ShouldBe(FileShareLinkStatus.Valid);

        FileShareTokens.CheckAgainstFile(resolved, Guid.NewGuid(), 0).ShouldBe(FileShareLinkStatus.Invalid);
        FileShareTokens.CheckAgainstFile(resolved, null, 0).ShouldBe(FileShareLinkStatus.Invalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("a.b.c")]
    public void Malformed_tokens_are_invalid(string? token)
    {
        FileShareTokens.Validate(Secret, token, Now, out _).ShouldBe(FileShareLinkStatus.Invalid);
    }

    [Fact]
    public void Tampered_or_foreign_secret_tokens_are_invalid()
    {
        var token = FileShareTokens.Create(Secret, NewPayload());
        var parts = token.Split('.');

        FileShareTokens.Validate(Secret, parts[0] + "." + parts[1][..^2] + "AA", Now, out _).ShouldBe(FileShareLinkStatus.Invalid);
        FileShareTokens.Validate("other-secret", token, Now, out _).ShouldBe(FileShareLinkStatus.Invalid);
    }

    [Fact]
    public void Media_access_tokens_cannot_be_used_as_share_tokens()
    {
        var media = new FileAccessTokenService(Microsoft.Extensions.Options.Options.Create(
            new SufiChain.SufiPlatform.FileManager.Configuration.FileManagerOptions { FileAccessTokenSecret = Secret }));

        FileShareTokens.Validate(Secret, media.GenerateToken(Guid.NewGuid()), Now, out _).ShouldBe(FileShareLinkStatus.Invalid);
    }
}