using System;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Communication;
using SufiChain.SufiPlatform.SufiCom.Communication.Repositories;
using SufiChain.SufiPlatform.SufiCom.Notifications;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Notifications;

public class ChannelMessageStatsIncrementHandlerTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private readonly ChannelMessageStatsIncrementHandler _handler;
    private readonly IChannelMessageStatsDailyRepository _statsRepository;

    public ChannelMessageStatsIncrementHandlerTests()
    {
        _handler = GetRequiredService<ChannelMessageStatsIncrementHandler>();
        _statsRepository = GetRequiredService<IChannelMessageStatsDailyRepository>();
    }

    [Fact]
    public async Task Should_Increment_Sent_And_Delivered_Counts()
    {
        var statDate = DateTime.UtcNow.Date;

        await _handler.HandleEventAsync(new ChannelMessageAuditChangedEto
        {
            TenantId = null,
            AuditId = Guid.NewGuid(),
            NotificationId = Guid.NewGuid(),
            Channel = NotificationChannels.Sms,
            NewState = ChannelMessageAuditState.Sent,
            StatDate = statDate
        });

        await _handler.HandleEventAsync(new ChannelMessageAuditChangedEto
        {
            TenantId = null,
            AuditId = Guid.NewGuid(),
            NotificationId = Guid.NewGuid(),
            Channel = NotificationChannels.Sms,
            PreviousState = ChannelMessageAuditState.Sent,
            NewState = ChannelMessageAuditState.Delivered,
            StatDate = statDate
        });

        var summaries = await _statsRepository.SumByChannelAsync(null, statDate, statDate.AddDays(1));
        var sms = summaries.ShouldHaveSingleItem();
        sms.Channel.ShouldBe(NotificationChannels.Sms);
        sms.SentCount.ShouldBe(1);
        sms.DeliveredCount.ShouldBe(1);
    }
}
