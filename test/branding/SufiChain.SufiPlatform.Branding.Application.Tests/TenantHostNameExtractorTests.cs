using System.Collections.Generic;
using Shouldly;
using SufiChain.SufiPlatform.Branding.Tenancy;
using Xunit;

namespace SufiChain.SufiPlatform.Branding;

public class TenantHostNameExtractorTests
{
    private static readonly IReadOnlySet<string> Reserved =
        TenantHostNameExtractor.CreateReservedLabelSet();

    [Theory]
    [InlineData("acme.sufichain.com", "{0}.sufichain.com", "acme")]
    [InlineData("stage.acme.sufichain.com", "stage.{0}.sufichain.com", "acme")]
    [InlineData("STAGE.ACME.SUFICHAIN.COM", "stage.{0}.sufichain.com", "ACME")]
    public void Extracts_a_single_tenant_label(string host, string format, string expected)
    {
        TenantHostNameExtractor.TryExtractTenantLabel(host, format, Reserved, out var label)
            .ShouldBeTrue();
        label.ShouldBe(expected);
    }

    [Theory]
    [InlineData("stage.console.sufichain.com", "stage.{0}.sufichain.com")]
    [InlineData("console.sufichain.com", "{0}.sufichain.com")]
    [InlineData("stage.console.sufichain.com", "{0}.sufichain.com")]
    [InlineData("stage.console.sufichain.com:443", "stage.{0}.sufichain.com")]
    [InlineData("stage.console.sufichain.com", "stage.console.sufichain.com")]
    public void Does_not_treat_the_console_host_or_a_multi_label_match_as_a_tenant(string host, string format)
    {
        TenantHostNameExtractor.TryExtractTenantLabel(host, format, Reserved, out var label)
            .ShouldBeFalse();
        label.ShouldBeEmpty();
    }
}
