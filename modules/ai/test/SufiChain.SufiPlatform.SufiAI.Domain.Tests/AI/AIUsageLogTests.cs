using System;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class AIUsageLogTests : SufiAITestBase<SufiAIDomainTestModule>
{
    [Fact]
    public void Should_Set_File_Info()
    {
        var usageLog = new AIUsageLog(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AICapabilityType.VisionAnalysis,
            "gpt-4o",
            AIProviderType.OpenAI);
        var fileId = Guid.NewGuid();
        const string fileUrl = "/api/file-manager/files/test";

        usageLog.SetFileInfo(fileId, fileUrl);

        usageLog.FileId.ShouldBe(fileId);
        usageLog.FileUrl.ShouldBe(fileUrl);
    }
}
