namespace SufiChain.Chat.Usage;

public class ChatUsageWalletContextDto
{
    public Guid? WalletId { get; set; }

    public string? WalletProviderName { get; set; }

    public string? BillingSubjectType { get; set; }

    public string? BillingSubjectId { get; set; }

    public bool IsChargeRequired { get; set; }

    public string? Currency { get; set; }
}
