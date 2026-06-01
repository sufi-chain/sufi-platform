namespace SufiChain.Chat.Usage;

public class ChatUsageCheckResult
{
    public bool IsAllowed { get; protected set; }

    public string? ReasonCode { get; protected set; }

    public string? LocalizationKey { get; protected set; }

    public LimitExceededAction? Action { get; protected set; }

    public bool RequiresAuthentication { get; protected set; }

    public static ChatUsageCheckResult Allowed()
    {
        return new ChatUsageCheckResult
        {
            IsAllowed = true
        };
    }

    public static ChatUsageCheckResult Denied(
        string reasonCode,
        string localizationKey,
        LimitExceededAction action = LimitExceededAction.BlockSend,
        bool requiresAuthentication = false)
    {
        return new ChatUsageCheckResult
        {
            IsAllowed = false,
            ReasonCode = reasonCode,
            LocalizationKey = localizationKey,
            Action = action,
            RequiresAuthentication = requiresAuthentication
        };
    }
}
