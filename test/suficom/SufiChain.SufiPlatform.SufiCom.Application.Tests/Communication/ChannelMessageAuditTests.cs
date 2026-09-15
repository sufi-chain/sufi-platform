using System;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Communication;
using SufiChain.SufiPlatform.SufiCom.Communication.Entities;
using SufiChain.SufiPlatform.SufiCom.Notifications;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Communication;

public class ChannelMessageAuditTests
{
    [Fact]
    public void Should_Transition_From_Queued_To_Sent()
    {
        var audit = new ChannelMessageAudit(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationChannels.Sms,
            ChannelMessageAuditState.Queued,
            recipientAddress: "+989121234567");

        audit.MarkSending();
        audit.MarkSent("ext-1");

        audit.State.ShouldBe(ChannelMessageAuditState.Sent);
        audit.ExternalId.ShouldBe("ext-1");
    }

    [Fact]
    public void Should_Not_Transition_From_Delivered()
    {
        var audit = new ChannelMessageAudit(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationChannels.InApp,
            ChannelMessageAuditState.Delivered);

        Should.Throw<BusinessException>(() => audit.MarkFailed("late failure"));
    }
}
