using System;
using System.Text;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileItems;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

public class FileAccessTokenServiceTests
{
    [Fact]
    public void Tampered_Token_Should_Not_Validate()
    {
        var sut = CreateSut();
        var fileId = Guid.NewGuid();
        var token = sut.GenerateToken(fileId);
        var parts = token.Split('.');
        var tampered = parts[0] + "." + parts[1][..^2] + "aa";

        sut.TryValidateToken(tampered, out _).ShouldBeFalse();
    }

    [Fact]
    public void Token_For_One_File_Should_Not_Authorize_Another()
    {
        var sut = CreateSut();
        var fileId = Guid.NewGuid();
        var token = sut.GenerateToken(fileId);

        sut.TryValidateToken(token, out var parsed).ShouldBeTrue();
        parsed.ShouldBe(fileId);
        parsed.ShouldNotBe(Guid.NewGuid());
    }

    [Fact]
    public void Expired_Token_Should_Not_Validate()
    {
        var sut = CreateSut(validityMinutes: -1);
        var token = sut.GenerateToken(Guid.NewGuid());

        sut.TryValidateToken(token, out _).ShouldBeFalse();
    }

    [Fact]
    public void Missing_Secret_Should_Not_Generate_Or_Validate()
    {
        var sut = new FileAccessTokenService(Options.Create(new FileManagerOptions
        {
            FileAccessTokenSecret = " "
        }));

        sut.IsConfigured.ShouldBeFalse();
        sut.TryGenerateToken(Guid.NewGuid(), out var token).ShouldBeFalse();
        token.ShouldBeEmpty();
        sut.TryValidateToken("a.b", out _).ShouldBeFalse();
    }

    [Fact]
    public void Payload_Without_Signature_Separator_Should_Not_Validate()
    {
        var sut = CreateSut();
        var junk = Convert.ToBase64String(Encoding.UTF8.GetBytes("not-a-token"));

        sut.TryValidateToken(junk, out _).ShouldBeFalse();
    }

    private static FileAccessTokenService CreateSut(int validityMinutes = 15)
    {
        return new FileAccessTokenService(Options.Create(new FileManagerOptions
        {
            FileAccessTokenSecret = "unit-test-file-access-secret",
            FileAccessTokenValidityMinutes = validityMinutes
        }));
    }
}
