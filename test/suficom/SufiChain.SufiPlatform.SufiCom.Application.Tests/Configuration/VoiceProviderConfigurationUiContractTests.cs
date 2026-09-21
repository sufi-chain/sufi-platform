using System;
using System.IO;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Configuration;

public class VoiceProviderConfigurationUiContractTests
{
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
                "VoiceProviderConfiguration.razor.cs");

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
                "VoiceProviderConfiguration.razor.cs");

            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(
            "VoiceProviderConfiguration.razor.cs was not found from the test output directory.");
    }
}
