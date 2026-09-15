using System;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Communication;
using SufiChain.SufiPlatform.SufiCom.Communication.Entities;
using SufiChain.SufiPlatform.SufiCom.Communication.Repositories;
using SufiChain.SufiPlatform.SufiCom.Notifications;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Communication;

public class MessageLogAppServiceTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private readonly IMessageLogAppService _messageLogAppService;
    private readonly IChannelMessageAuditRepository _auditRepository;

    public MessageLogAppServiceTests()
    {
        _messageLogAppService = GetRequiredService<IMessageLogAppService>();
        _auditRepository = GetRequiredService<IChannelMessageAuditRepository>();
    }

    [Fact]
    public async Task Should_Get_Paged_Sms_Logs()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _auditRepository.InsertAsync(new ChannelMessageAudit(
                Guid.NewGuid(),
                Guid.NewGuid(),
                NotificationChannels.Sms,
                ChannelMessageAuditState.Sent,
                renderedContent: "Test message",
                recipientAddress: "+989121234567",
                providerCode: "Kavenegar",
                priority: MessagePriority.Normal));
        });

        var result = await _messageLogAppService.GetListAsync(new GetMessageLogsInput
        {
            MaxResultCount = 10,
            SkipCount = 0,
            MessageType = SufiChain.SufiPlatform.SufiCom.Communication.MessageType.Sms
        });

        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(x => x.Content == "Test message");
    }
}
