using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Tickets;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.Ticketing.Tickets;

public class TicketAssignmentWorkflowTests
{
    [Fact]
    public void Assign_Should_Move_A_New_Ticket_To_Assigned()
    {
        var ticket = NewTicket();
        var agentId = Guid.NewGuid();
        ticket.Assign(agentId, null, DateTime.UtcNow);

        ticket.Status.ShouldBe(TicketStatus.Assigned);
        ticket.AssignedAgentId.ShouldBe(agentId);
    }

    [Fact]
    public void Assign_Should_Reject_A_Ticket_That_Is_Not_New()
    {
        var ticket = NewTicket();
        ticket.Assign(Guid.NewGuid(), null, DateTime.UtcNow);
        var error = Should.Throw<BusinessException>(() => ticket.Assign(Guid.NewGuid(), null, DateTime.UtcNow));
        error.Code.ShouldBe(TicketingErrorCodes.InvalidStatusTransition);
    }

    private static Ticket NewTicket() =>
        new(Guid.NewGuid(), "T-1", "Subject", "Description", TicketPriority.Normal, null, null, null);
}
