using SufiChain.Chat.Supports;
using SufiChain.Chat.Usage;
using Shouldly;
using Xunit;

namespace SufiChain.Chat.Usage;

public class ChatWalletResolver_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    [Fact]
    public async Task Should_Return_Null_Wallet_Context_By_Default()
    {
        var resolver = GetRequiredService<IChatUsageWalletResolver>();
        var context = await resolver.ResolveAsync(new ChatAiOperationContext
        {
            SessionId = Guid.NewGuid(),
            OperationKind = ChatAiOperationKind.AutoReply
        });

        context.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Use_Replaced_Wallet_Resolver()
    {
        var testResolver = GetRequiredService<TestChatUsageWalletResolver>();
        testResolver.Context = new ChatUsageWalletContext
        {
            WalletId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            WalletProviderName = "TestWallet",
            BillingSubjectType = "Tenant",
            BillingSubjectId = ChatTestData.TenantAId.ToString("D"),
            IsChargeRequired = true,
            Currency = "USD"
        };

        var usageGuard = (ChatUsageGuard)GetRequiredService<IChatUsageGuard>();
        var reservationId = await usageGuard.ReserveAiUsageAsync(new ChatAiOperationContext
        {
            TenantId = ChatTestData.TenantAId,
            SessionId = Guid.NewGuid(),
            UserId = ChatTestData.UserAId,
            AccessMode = AccessMode.PublicAuthenticated,
            ConversationKind = ConversationKind.Assistant,
            OperationKind = ChatAiOperationKind.AutoReply,
            ProviderName = "openai",
            WorkspaceName = ChatTestData.DefaultWorkspaceName
        });

        var reservation = await GetRequiredService<IChatAiUsageReservationRepository>().GetAsync(reservationId);
        reservation.WalletId.ShouldBe(testResolver.Context.WalletId);
        reservation.WalletProviderName.ShouldBe("TestWallet");
        reservation.ProviderName.ShouldBe("openai");
        reservation.WorkspaceName.ShouldBe(ChatTestData.DefaultWorkspaceName);
        reservation.TenantId.ShouldBe(ChatTestData.TenantAId);
        reservation.UserId.ShouldBe(ChatTestData.UserAId);
        reservation.OperationKind.ShouldBe(ChatAiOperationKind.AutoReply);
    }
}
