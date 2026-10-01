using Shouldly;
using SufiChain.SufiPlatform.HelpDesk;
using SufiChain.SufiPlatform.HelpDesk.LiveChat;
using SufiChain.SufiPlatform.HelpDesk.LiveChat.Handoff;
using SufiChain.SufiPlatform.HelpDesk.LiveChat.Sessions;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.LiveChat.Sessions;

public class LiveChatSessionHandoffTests : AbpIntegratedTest<HelpDeskLiveChatTestModule>
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    [Fact]
    public void AssignToOperator_Should_Take_The_Session()
    {
        GetRequiredService<Volo.Abp.Guids.IGuidGenerator>().Create().ShouldNotBe(Guid.Empty);
        var profile = NewProfile();
        var operatorId = Guid.NewGuid();

        profile.AssignToOperator(operatorId, LiveChatHandoffReason.OperatorClaimed, DateTime.UtcNow);

        profile.HandoffMode.ShouldBe(HandoffMode.Operator);
        profile.AssignedOperatorId.ShouldBe(operatorId);
        profile.AiWorkspaceId.ShouldBeNull();
    }

    [Fact]
    public void AssignToOperator_Should_Reject_An_Empty_Operator()
    {
        var profile = NewProfile();
        var error = Should.Throw<BusinessException>(() =>
            profile.AssignToOperator(Guid.Empty, LiveChatHandoffReason.OperatorClaimed, DateTime.UtcNow));
        error.Code.ShouldBe(LiveChatErrorCodes.OperatorNotAvailable);
        profile.HandoffMode.ShouldBe(HandoffMode.Waiting);
    }

    private static LiveChatSessionProfile NewProfile() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true, "Visitor", null);
}
