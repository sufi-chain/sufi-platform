using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Permissions;
using SufiChain.SufiPlatform.SufiForms.Records;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormDefinitionPublishTests
{
    [Fact]
    public void Publish_requires_SufiForms_Forms_Publish()
    {
        var authorize = typeof(FormDefinitionAppService)
            .GetMethod(nameof(FormDefinitionAppService.PublishAsync))!
            .GetCustomAttribute<AuthorizeAttribute>();

        authorize.ShouldNotBeNull();
        authorize.Policy.ShouldBe(SufiFormsPermissions.Forms.Publish);
    }

    [Fact]
    public async Task Draft_record_save_is_blocked_until_the_definition_is_published()
    {
        var records = new FormRecordManager { LazyServiceProvider = LazyGuids() };
        var definitions = new FormDefinitionManager(Substitute.For<IFormDefinitionRepository>());
        var definition = new FormDefinition(Guid.NewGuid(), null, "intake", "Intake", "Intake");
        definition.AddField(new FormField(Guid.NewGuid(), "title", FormFieldType.Text, "Title"));
        var values = new Dictionary<string, object?> { ["title"] = "hello" };

        var blocked = await Should.ThrowAsync<BusinessException>(() => records.CreateAsync(definition, values));
        blocked.Code.ShouldBe(SufiFormsErrorCodes.FormDefinitionNotPublished);

        await definitions.PublishAsync(definition);
        definition.PublishState.ShouldBe(FormPublishState.Published);

        var record = await records.CreateAsync(definition, values);
        record.FormKey.ShouldBe("intake");
        record.ValuesJson.ShouldContain("hello");

        await definitions.UnpublishAsync(definition);
        definition.PublishState.ShouldBe(FormPublishState.Draft);

        var blockedAgain = await Should.ThrowAsync<BusinessException>(() => records.CreateAsync(definition, values));
        blockedAgain.Code.ShouldBe(SufiFormsErrorCodes.FormDefinitionNotPublished);
    }

    private static IAbpLazyServiceProvider LazyGuids()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
        return new AbpLazyServiceProvider(services.BuildServiceProvider());
    }
}
