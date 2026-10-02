using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class GuardModelReplyTests
{
    [Theory]
    [InlineData("User Safety: safe Response Safety: safe")]
    [InlineData("  User Safety: safe Response Safety: safe  ")]
    [InlineData("User Safety: unsafe")]
    [InlineData("User Safety: safe\nResponse Safety: unsafe")]
    public void Classifier_verdicts_match(string content)
    {
        GuardModelReply.IsVerdict(content).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("User Safety: safe. Here is the answer.")]
    [InlineData("سلام! چطور می‌تونم کمکت کنم؟")]
    public void Ordinary_replies_do_not_match(string? content)
    {
        GuardModelReply.IsVerdict(content).ShouldBeFalse();
    }
}
