using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.AiUsage;

public class GetChatAiUsageDashboardInput
{
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public Guid? SessionId { get; set; }
}

public class GetChatAiUsageReservationsInput : PagedAndSortedResultRequestDto
{
    public Guid? SessionId { get; set; }

    public Guid? UserId { get; set; }

    public ChatAiOperationKind? OperationKind { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
}

public class GetChatAiUsageRecordsInput : PagedAndSortedResultRequestDto
{
    public Guid? SessionId { get; set; }

    public Guid? UserId { get; set; }

    public ChatAiOperationKind? OperationKind { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
}
