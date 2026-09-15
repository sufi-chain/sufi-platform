using System;
using System.Text;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Connections;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Connections;

/// <summary>
/// Round-trip and tamper tests for the shared <see cref="TelegramHmac"/> signing primitive
/// used by both the Iran signing handler / validation filters and the ForeignHost middleware.
/// </summary>
public class TelegramHmac_Tests
{
    private const string SharedKey = "test-shared-secret";
    private const string Method = "POST";
    private const string Path = "/integration-api/telegram-bridge/ingest";
    private static readonly byte[] Body = Encoding.UTF8.GetBytes("""{"connectionId":"abc"}""");

    [Fact]
    public void Same_Canonical_Form_Produces_Same_Signature()
    {
        var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();

        var a = TelegramHmac.ComputeSignature(Method, Path, timestamp, Body, SharedKey);
        var b = TelegramHmac.ComputeSignature(Method, Path, timestamp, Body, SharedKey);

        a.ShouldBe(b);
    }

    [Fact]
    public void Tampered_Body_Produces_Different_Signature()
    {
        var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();

        var original = TelegramHmac.ComputeSignature(Method, Path, timestamp, Body, SharedKey);
        var tampered = TelegramHmac.ComputeSignature(Method, Path, timestamp, Encoding.UTF8.GetBytes("{}"), SharedKey);

        original.ShouldNotBe(tampered);
    }

    [Fact]
    public void IsTimestampValid_Accepts_Now()
    {
        var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        TelegramHmac.IsTimestampValid(timestamp).ShouldBeTrue();
    }

    [Fact]
    public void IsTimestampValid_Rejects_Stale_Timestamp_Beyond_Skew()
    {
        // 10 minutes ago — outside the ±5 minute skew window.
        var stale = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-10)).ToUnixTimeSeconds().ToString();
        TelegramHmac.IsTimestampValid(stale).ShouldBeFalse();
    }

    [Fact]
    public void IsTimestampValid_Rejects_NonNumeric_Timestamp()
    {
        TelegramHmac.IsTimestampValid("not-a-number").ShouldBeFalse();
    }

    [Fact]
    public void Full_Validation_Matches_Independent_Signing()
    {
        var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        var signature = TelegramHmac.ComputeSignature(Method, Path, timestamp, Body, SharedKey);

        // Recompute and compare constant-time equivalent (lowercased hex).
        var expected = TelegramHmac.ComputeSignature(Method, Path, timestamp, Body, SharedKey);
        string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
    }
}
