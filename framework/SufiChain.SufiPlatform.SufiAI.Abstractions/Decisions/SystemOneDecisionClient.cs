namespace SufiChain.SufiPlatform.SufiAI;

public sealed class SystemOneQuestion
{
    public string Type { get; set; } = "noul";

    public string Instructions { get; set; } = string.Empty;

    public object? Criteria { get; set; }
}

public sealed class SystemOneDecisionRequest
{
    public Guid WorkspaceId { get; set; }

    public object? State { get; set; }

    public IReadOnlyDictionary<string, SystemOneQuestion> Questions { get; set; }
        = new Dictionary<string, SystemOneQuestion>();
}

public sealed class SystemOneAnswer
{
    public string Type { get; set; } = string.Empty;

    public double? Noul { get; set; }

    public string? Choice { get; set; }

    public double? Confidence { get; set; }
}

public sealed class SystemOneDecisionResult
{
    public const double ClearYes = 0.9;

    public const double ClearNo = 0.1;

    public IReadOnlyDictionary<string, SystemOneAnswer> Answers { get; set; }
        = new Dictionary<string, SystemOneAnswer>();

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }

    public decimal? Cost { get; set; }

    public static bool IsClearYes(SystemOneAnswer? answer)
    {
        return answer?.Type == "noul" && answer.Noul is >= ClearYes and <= 1;
    }

    public static bool IsClearNo(SystemOneAnswer? answer)
    {
        return answer?.Type == "noul" && answer.Noul is >= 0 and <= ClearNo;
    }

    public static bool IsClearChoice(SystemOneAnswer? answer)
    {
        return answer?.Type == "choice" &&
               !string.IsNullOrWhiteSpace(answer.Choice) &&
               answer.Confidence is >= ClearYes and <= 1;
    }
}

public interface ISystemOneDecisionClient
{
    Task<SystemOneDecisionResult?> TryDecideAsync(
        SystemOneDecisionRequest request,
        CancellationToken cancellationToken = default);
}
