using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
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
}
