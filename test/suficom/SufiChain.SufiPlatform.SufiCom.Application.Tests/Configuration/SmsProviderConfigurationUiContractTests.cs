using System;
using System.IO;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Configuration;

public class SmsProviderConfigurationUiContractTests
{
    [Fact]
    public void TestProviderAsync_Must_Keep_Submitted_Form_And_Not_Reload_Persisted_Provider()
    {
        var source = File.ReadAllText(FindPageSource());
        var methodStart = source.IndexOf("protected virtual Task TestProviderAsync()", StringComparison.Ordinal);
        methodStart.ShouldBeGreaterThan(-1);

        var methodEnd = source.IndexOf("protected virtual string GetCurrentProviderDisplayName()", methodStart, StringComparison.Ordinal);
        methodEnd.ShouldBeGreaterThan(methodStart);

        var method = source[methodStart..methodEnd];
        method.ShouldContain("ConfigurationService.TestProviderAsync(input)");
        method.ShouldContain("SelectedProviderCode");
        method.ShouldContain("ProviderSettings");
        method.ShouldContain("TestPhoneNumber");
        method.ShouldContain("TestMessage");
        method.ShouldNotContain("LoadDataAsync");
        method.ShouldNotContain("UpdateConfigurationAsync");
        method.ShouldNotContain("GetConfigurationAsync");
    }

    [Fact]
    public void CanSave_Must_Accept_Stored_Sensitive_Settings()
    {
        var source = File.ReadAllText(FindPageSource());
        source.ShouldContain("HasRequiredSettings(requireTestPhone: false)");
        source.ShouldContain("HasStoredSensitiveSettings");
        source.ShouldNotContain("ProviderSettings.ContainsKey(\"ApiKey\")");
    }

    private static string FindPageSource()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "pro-modules",
                "suficom",
                "src",
                "SufiChain.SufiPlatform.SufiCom.Blazor",
                "Pages",
                "Admin",
                "SmsProviderConfiguration.razor.cs");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(
                directory.FullName,
                "src",
                "SufiChain.SufiPlatform.SufiCom.Blazor",
                "Pages",
                "Admin",
                "SmsProviderConfiguration.razor.cs");

            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(
            "SmsProviderConfiguration.razor.cs was not found from the test output directory.");
    }
}
