using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Templates;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Templates;

public class MessageTemplateAppServiceTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private readonly IMessageTemplateAppService _templateAppService;

    public MessageTemplateAppServiceTests()
    {
        _templateAppService = GetRequiredService<IMessageTemplateAppService>();
    }

    [Fact]
    public async Task Should_Create_Template()
    {
        var input = new CreateMessageTemplateDto
        {
            TemplateKey = "Test.Welcome",
            Culture = "en",
            Channel = MessageChannel.SMS,
            Content = "Hello {Name}",
            Description = "Welcome message",
            Variables = new List<TemplateVariableDto>
            {
                new()
                {
                    Key = "Name",
                    Description = "Recipient name",
                    IsRequired = true,
                    SampleValue = "Alice"
                }
            }
        };

        var result = await WithUnitOfWorkAsync(() => _templateAppService.CreateAsync(input));

        result.Id.ShouldNotBe(Guid.Empty);
        result.TemplateKey.ShouldBe("Test.Welcome");
        result.Culture.ShouldBe("en");
        result.Channel.ShouldBe(MessageChannel.SMS);
        result.Content.ShouldBe("Hello {Name}");
    }

    [Fact]
    public async Task Should_Duplicate_Template()
    {
        var created = await WithUnitOfWorkAsync(() => _templateAppService.CreateAsync(new CreateMessageTemplateDto
        {
            TemplateKey = "Test.Source",
            Culture = "en",
            Channel = MessageChannel.SMS,
            Content = "Source content"
        }));

        var duplicate = await WithUnitOfWorkAsync(() =>
            _templateAppService.DuplicateAsync(created.Id, "Test.Copy", "fa"));

        duplicate.Id.ShouldNotBe(created.Id);
        duplicate.TemplateKey.ShouldBe("Test.Copy");
        duplicate.Culture.ShouldBe("fa");
        duplicate.Content.ShouldBe("Source content");
    }

    [Fact]
    public async Task Should_Preview_Template()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _templateAppService.CreateAsync(new CreateMessageTemplateDto
            {
                TemplateKey = "Test.Preview",
                Culture = "en",
                Channel = MessageChannel.SMS,
                Content = "Hello {Name}"
            });
        });

        var preview = await _templateAppService.PreviewAsync(new RenderTemplateInput
        {
            TemplateKey = "Test.Preview",
            Culture = "en",
            Data = new Dictionary<string, object> { ["Name"] = "Bob" }
        });

        preview.RenderedContent.ShouldContain("Bob");
    }

    [Fact]
    public async Task Should_Preview_Unknown_Scriban_Variable_As_Blank_With_Diagnostic()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _templateAppService.CreateAsync(new CreateMessageTemplateDto
            {
                TemplateKey = "Test.QaMissing",
                Culture = "en",
                Channel = MessageChannel.SMS,
                Content = "Hello {{ qa_missing }}"
            });
        });

        var stored = await _templateAppService.PreviewAsync(new RenderTemplateInput
        {
            TemplateKey = "Test.QaMissing",
            Culture = "en",
            Channel = MessageChannel.SMS
        });

        stored.RenderedContent.ShouldContain("Hello");
        stored.RenderedContent.ShouldNotContain("qa_missing");
        stored.MissingVariables.ShouldContain("qa_missing");

        var withoutValue = await _templateAppService.PreviewAsync(new RenderTemplateInput
        {
            TemplateKey = "Test.QaMissing",
            Culture = "en",
            Channel = MessageChannel.Email,
            ContentOverride = "Hello {{ qa_missing }}"
        });

        withoutValue.RenderedContent.ShouldContain("Hello");
        withoutValue.RenderedContent.ShouldNotContain("qa_missing");
        withoutValue.HasMissingVariables.ShouldBeTrue();
        withoutValue.MissingVariables.ShouldContain("qa_missing");

        var withValue = await _templateAppService.PreviewAsync(new RenderTemplateInput
        {
            TemplateKey = "Test.QaMissing",
            Culture = "en",
            Channel = MessageChannel.SMS,
            ContentOverride = "Hello {{ qa_missing }}",
            Data = new Dictionary<string, object> { ["qa_missing"] = "ready" }
        });

        withValue.RenderedContent.ShouldContain("ready");
        withValue.MissingVariables.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Preview_Parse_Error_As_Business_Exception_With_Location()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            _templateAppService.PreviewAsync(new RenderTemplateInput
            {
                TemplateKey = "Test.QaParse",
                Culture = "en",
                Channel = MessageChannel.Voice,
                ContentOverride = "{{ 1 + }}"
            }));

        exception.Code.ShouldBe(SufiComDomainErrorCodes.TemplateParseFailed);
        exception.Data["Line"].ShouldBe(1);
        exception.Data["Column"].ShouldBe(8);
        exception.Data["Detail"].ToString().ShouldContain("binary expression");
    }

    [Fact]
    public async Task Should_Preview_Runtime_Error_As_Business_Exception_With_Location()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            _templateAppService.PreviewAsync(new RenderTemplateInput
            {
                TemplateKey = "Test.QaRuntime",
                Culture = "en",
                Channel = MessageChannel.SMS,
                ContentOverride = "Hello {{ qa missing }}"
            }));

        exception.Code.ShouldBe(SufiComDomainErrorCodes.TemplateParseFailed);
        exception.Data["Line"].ShouldBe(1);
        exception.Data["Column"].ShouldBe(10);
    }
}
