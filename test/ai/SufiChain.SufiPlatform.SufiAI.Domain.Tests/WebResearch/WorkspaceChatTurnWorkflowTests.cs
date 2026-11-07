using SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.WebResearch;

/// <summary>
/// sufi-chain#551: the workspace page, not the attempt helper alone, decides which operation a save uses.
/// </summary>
public class WorkspaceChatTurnWorkflowTests
{
    [Fact]
    public void A_new_identical_reply_after_a_failed_save_is_a_new_turn()
    {
        var page = new WorkspaceChat();
        var sessionId = Guid.NewGuid();
        var firstTurn = Guid.NewGuid();
        const string reply = "the same assistant text";

        var failed = page.OperationIdForSave(sessionId, firstTurn, reply);
        page.MarkAssistantDeliveryFailed();

        var next = page.OperationIdForSave(sessionId, Guid.NewGuid(), reply);

        Assert.NotEqual(failed, next);
        Assert.Throws<InvalidOperationException>(() => page.OperationIdForRetry(sessionId, firstTurn));
    }

    [Fact]
    public void Switching_sessions_after_a_failure_does_not_reuse_the_operation()
    {
        var page = new WorkspaceChat();
        var firstSession = Guid.NewGuid();
        var secondSession = Guid.NewGuid();
        var turnId = Guid.NewGuid();
        const string reply = "kept reply";

        var failed = page.OperationIdForSave(firstSession, turnId, reply);
        page.MarkAssistantDeliveryFailed();
        page.BindSelectedSession(secondSession);

        var switched = page.OperationIdForSave(secondSession, Guid.NewGuid(), reply);

        Assert.NotEqual(failed, switched);
        Assert.Throws<InvalidOperationException>(() => page.OperationIdForRetry(firstSession, turnId));
        Assert.Throws<InvalidOperationException>(() => page.OperationIdForRetry(secondSession, turnId));
    }

    [Fact]
    public void Retrying_the_failed_turn_is_a_separate_call()
    {
        var page = new WorkspaceChat();
        var sessionId = Guid.NewGuid();
        var turnId = Guid.NewGuid();
        const string reply = "retry this reply";

        var failed = page.OperationIdForSave(sessionId, turnId, reply);
        page.MarkAssistantDeliveryFailed();

        Assert.Equal(failed, page.OperationIdForRetry(sessionId, turnId));
        page.MarkAssistantDeliveryStored();
        Assert.Throws<InvalidOperationException>(() => page.OperationIdForRetry(sessionId, turnId));
    }
}
