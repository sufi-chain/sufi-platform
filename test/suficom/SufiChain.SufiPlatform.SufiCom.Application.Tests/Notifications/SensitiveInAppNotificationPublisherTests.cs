using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiCom.Communication.Repositories;
using SufiChain.SufiPlatform.SufiCom.Inbox;
using SufiChain.SufiPlatform.SufiCom.Notifications;
using SufiChain.SufiPlatform.SufiCom.Templates;
using Volo.Abp.Localization;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Notifications;

public class SensitiveInAppNotificationPublisherTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private readonly ISensitiveInAppNotificationPublisher _publisher;
    private readonly IInboxNotificationRepository _inboxRepository;
    private readonly IChannelMessageAuditRepository _auditRepository;
    private readonly IMessageTemplateAppService _templateAppService;
    private readonly ISettingManager _settingManager;

    public SensitiveInAppNotificationPublisherTests()
    {
        _publisher = GetRequiredService<ISensitiveInAppNotificationPublisher>();
        _inboxRepository = GetRequiredService<IInboxNotificationRepository>();
        _auditRepository = GetRequiredService<IChannelMessageAuditRepository>();
        _templateAppService = GetRequiredService<IMessageTemplateAppService>();
        _settingManager = GetRequiredService<ISettingManager>();
    }

    [Fact]
    public async Task Should_Deliver_Once_To_One_Host_User_Without_Audit_Copy()
    {
        var notificationId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var templateName = $"Test.Sensitive.{notificationId:N}";

        await WithUnitOfWorkAsync(() => _templateAppService.CreateAsync(new CreateMessageTemplateDto
        {
            TemplateKey = templateName,
            Culture = "en",
            Channel = MessageChannel.Push,
            Content = "Workspace {{tenantName}}. Username: {{userName}} Password: {{password}}"
        }));

        var request = new SensitiveInAppNotificationRequest
        {
            NotificationId = notificationId,
            UserId = ownerUserId,
            Title = "Workspace ready",
            TemplateName = templateName,
            Source = "SufiSaas",
            Category = NotificationCategories.System,
            TemplateData = new Dictionary<string, object>
            {
                ["tenantName"] = "IR Needs",
                ["tenantHost"] = "irneeds.sufichain.com",
                ["userName"] = "admin",
                ["password"] = "temporary-secret",
                ["loginUrl"] = "https://irneeds.sufichain.com"
            }
        };

        (await _publisher.PublishAsync(request)).ShouldBeTrue();
        (await _publisher.PublishAsync(request)).ShouldBeFalse();

        await WithUnitOfWorkAsync(async () =>
        {
            var inboxQuery = await _inboxRepository.GetQueryableAsync();
            var notifications = inboxQuery
                .Where(notification => notification.NotificationId == notificationId)
                .ToList();

            notifications.Count.ShouldBe(1);
            notifications[0].TenantId.ShouldBeNull();
            notifications[0].UserId.ShouldBe(ownerUserId);
            notifications[0].Body.ShouldContain("temporary-secret");
            notifications[0].Category.ShouldBe(NotificationCategories.System);

            var auditQuery = await _auditRepository.GetQueryableAsync();
            auditQuery.Any(audit => audit.NotificationId == notificationId).ShouldBeFalse();
        });
    }

    [Fact]
    public async Task Should_Render_With_Explicit_Recipient_Language_Preference()
    {
        var notificationId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var templateName = $"Test.Sensitive.Culture.{notificationId:N}";

        await CreateLocalizedTemplatesAsync(templateName);
        await WithUnitOfWorkAsync(() => _settingManager.SetForUserAsync(
            ownerUserId,
            SufiLocalizationSettingNames.DefaultLanguage,
            "fa"));

        using (CultureHelper.Use("en"))
        {
            (await _publisher.PublishAsync(CreateCultureRequest(
                notificationId,
                ownerUserId,
                templateName))).ShouldBeTrue();
        }

        await WithUnitOfWorkAsync(async () =>
        {
            var inboxQuery = await _inboxRepository.GetQueryableAsync();
            var notification = inboxQuery.Single(item => item.NotificationId == notificationId);
            notification.Body.ShouldBe("فضای کاری آماده است");
        });
    }

    [Fact]
    public async Task Explicit_Request_Culture_Should_Override_Recipient_Preference()
    {
        var notificationId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var templateName = $"Test.Sensitive.Culture.Override.{notificationId:N}";

        await CreateLocalizedTemplatesAsync(templateName);
        await WithUnitOfWorkAsync(() => _settingManager.SetForUserAsync(
            ownerUserId,
            SufiLocalizationSettingNames.DefaultLanguage,
            "fa"));

        var request = CreateCultureRequest(notificationId, ownerUserId, templateName);
        request.Culture = "en";

        (await _publisher.PublishAsync(request)).ShouldBeTrue();

        await WithUnitOfWorkAsync(async () =>
        {
            var inboxQuery = await _inboxRepository.GetQueryableAsync();
            var notification = inboxQuery.Single(item => item.NotificationId == notificationId);
            notification.Body.ShouldBe("Workspace is ready");
        });
    }

    [Fact]
    public async Task Should_Use_First_Configured_Language_When_No_Preference_Exists()
    {
        var notificationId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var templateName = $"Test.Sensitive.Culture.Default.{notificationId:N}";

        await CreateLocalizedTemplatesAsync(templateName);

        using (CultureHelper.Use("en"))
        {
            (await _publisher.PublishAsync(CreateCultureRequest(
                notificationId,
                ownerUserId,
                templateName))).ShouldBeTrue();
        }

        await WithUnitOfWorkAsync(async () =>
        {
            var inboxQuery = await _inboxRepository.GetQueryableAsync();
            var notification = inboxQuery.Single(item => item.NotificationId == notificationId);
            notification.Body.ShouldBe("فضای کاری آماده است");
        });
    }

    private async Task CreateLocalizedTemplatesAsync(string templateName)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _templateAppService.CreateAsync(new CreateMessageTemplateDto
            {
                TemplateKey = templateName,
                Culture = "en",
                Channel = MessageChannel.Push,
                Content = "Workspace is ready"
            });

            await _templateAppService.CreateAsync(new CreateMessageTemplateDto
            {
                TemplateKey = templateName,
                Culture = "fa",
                Channel = MessageChannel.Push,
                Content = "فضای کاری آماده است"
            });
        });
    }

    private static SensitiveInAppNotificationRequest CreateCultureRequest(
        Guid notificationId,
        Guid ownerUserId,
        string templateName)
    {
        return new SensitiveInAppNotificationRequest
        {
            NotificationId = notificationId,
            UserId = ownerUserId,
            Title = "Workspace ready",
            TemplateName = templateName,
            Source = "SufiSaas",
            Category = NotificationCategories.System
        };
    }
}
