using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Ai;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Copilots;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Tickets;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk;

public class TicketCopilotContextBuilderTests
{
    [Fact]
    public void AgentReply_Context_Should_Include_Instruction_And_Omit_Tone()
    {
        var ticket = CreateTicket();
        var context = TicketCopilotContextBuilder.BuildAgentReplyContext(ticket, ticket.Id, "Suggest reply");

        context[HelpDeskAgentReplyCopilotKeys.Context.TicketId].ShouldBe(ticket.Id.ToString("D"));
        context[HelpDeskAgentReplyCopilotKeys.Context.Instruction].ShouldBe("Suggest reply");
        context.ContainsKey("tone").ShouldBeFalse();
        context[HelpDeskAgentReplyCopilotKeys.Context.Thread].ShouldContain("[Visitor] Need help");
    }

    [Fact]
    public void Triage_Context_Should_Include_Priority_And_Omit_Queue_And_Sla()
    {
        var ticket = CreateTicket();
        var context = TicketCopilotContextBuilder.BuildTriageContext(ticket, ticket.Id);

        context[HelpDeskTicketTriageCopilotKeys.Context.Priority].ShouldBe(TicketPriority.High.ToString());
        context.ContainsKey("queue").ShouldBeFalse();
        context.ContainsKey("sla").ShouldBeFalse();
    }

    [Fact]
    public void Thread_Should_Format_Sender_And_Body()
    {
        var ticket = CreateTicket();
        var thread = TicketCopilotContextBuilder.BuildThreadText(ticket);

        thread.ShouldBe("[Visitor] Need help");
    }

    private static Ticket CreateTicket()
    {
        var ticket = new Ticket(
            Guid.NewGuid(),
            "T-1",
            "Cannot sign in",
            "Password reset failed",
            TicketPriority.High,
            requesterUserId: null,
            requesterContact: "user@example.com",
            tenantId: null,
            projectId: Guid.NewGuid());

        ticket.AddComment(
            Guid.NewGuid(),
            HelpDeskMessageSenderKind.Visitor,
            "Need help",
            isInternalNote: false,
            occurredAt: DateTime.UtcNow);

        return ticket;
    }
}
