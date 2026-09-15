using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotDefinitionOverrideTests
{
    [Fact]
    public void Should_Preserve_Seed_Version_And_Fingerprint_When_Updating_Override()
    {
        var definitionOverride = new CopilotDefinitionOverride(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            """{"systemPrompt":"Initial"}""",
            3,
            "seed-fingerprint-3");

        definitionOverride.SetConfiguration(
            """{"systemPrompt":"Updated"}""",
            5,
            "seed-fingerprint-5");

        definitionOverride.ConfigurationJson.ShouldContain("Updated");
        definitionOverride.BaseSeedEntityVersion.ShouldBe(5);
        definitionOverride.BaseSeedConfigurationFingerprint.ShouldBe("seed-fingerprint-5");
        definitionOverride.IsSeedStale(5, "seed-fingerprint-5").ShouldBeFalse();
        definitionOverride.IsSeedStale(6, "seed-fingerprint-5").ShouldBeTrue();
        definitionOverride.IsSeedStale(5, "seed-fingerprint-6").ShouldBeTrue();
    }
}
