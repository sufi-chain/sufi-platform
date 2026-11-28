using System.Text.Json;
using SufiChain.SufiPlatform.SufiAI;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests;

public class AiCapabilityLocalizationTests
{
    [Fact]
    public void Every_Capability_Has_Fa_And_En_Text()
    {
        var en = Read("en");
        var fa = Read("fa");
        foreach (var capability in Enum.GetValues<AICapabilityType>())
        {
            var key = AiUsageErrorText.CapabilityKey(capability);
            en[key].ShouldNotBeNullOrWhiteSpace();
            fa[key].ShouldNotBeNullOrWhiteSpace();
            fa[key].ShouldNotBe(capability.ToString());
        }
    }

    [Fact]
    public void UsageLog_KnownErrorCode_ShowsLocalizedText()
    {
        AiUsageErrorText.IsKnownCode("SystemOneRequestFailed").ShouldBeTrue();
        Read("fa")["SystemOneRequestFailed"].ShouldNotBe("SystemOneRequestFailed");
        Read("en")["SystemOneRequestFailed"].ShouldNotBe("SystemOneRequestFailed");
    }

    private static Dictionary<string, string> Read(string culture)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Domain.Shared/Localization/AI/en.json")))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull();
        var path = Path.Combine(dir!.FullName, "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Domain.Shared/Localization/AI", culture + ".json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.GetProperty("texts").EnumerateObject()
            .ToDictionary(item => item.Name, item => item.Value.GetString() ?? string.Empty);
    }
}
