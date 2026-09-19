using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvarePlaygroundSessionStateTests
{
    [Fact]
    public void Should_Update_Draft_And_Last_Preview()
    {
        var state = new HooshvarePlaygroundSessionState(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            """{"systemPrompt":"Initial"}""",
            "base-fingerprint");

        state.UpdateDraft(
            """{"systemPrompt":"Updated"}""",
            "updated-fingerprint");
        state.SetLastPreview("""{"message":"Preview"}""");

        state.DraftConfigurationJson.ShouldContain("Updated");
        state.BaseConfigurationFingerprint.ShouldBe("updated-fingerprint");
        state.LastPreviewJson!.ShouldContain("Preview");
    }

    [Fact]
    public void Should_Clear_Last_Preview_When_Value_Is_Empty()
    {
        var state = new HooshvarePlaygroundSessionState(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "{}",
            "base-fingerprint");

        state.SetLastPreview("{}");
        state.SetLastPreview(" ");

        state.LastPreviewJson.ShouldBeNull();
    }
}
