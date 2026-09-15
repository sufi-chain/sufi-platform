using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotRagProjectBindingTests
{
    [Fact]
    public void Should_Normalize_Source_And_Start_Enabled()
    {
        var binding = new CopilotRagProjectBinding(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            " KnowledgeBase ");

        binding.SourceName.ShouldBe("KnowledgeBase");
        binding.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Should_Toggle_Enabled_State()
    {
        var binding = new CopilotRagProjectBinding(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "KnowledgeBase");

        binding.Disable();
        binding.IsEnabled.ShouldBeFalse();

        binding.Enable();
        binding.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Should_Reject_Empty_Identity()
    {
        Should.Throw<ArgumentException>(() => new CopilotRagProjectBinding(
            Guid.NewGuid(),
            null,
            Guid.Empty,
            Guid.NewGuid(),
            "KnowledgeBase"));

        Should.Throw<ArgumentException>(() => new CopilotRagProjectBinding(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.Empty,
            "KnowledgeBase"));
    }
}
