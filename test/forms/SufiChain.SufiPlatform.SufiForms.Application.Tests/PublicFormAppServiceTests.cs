using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Etos;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Permissions;
using SufiChain.SufiPlatform.SufiForms.Records;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class PublicFormAppServiceTests
{
    private readonly Guid _tenantId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private readonly IFormDefinitionRepository _definitions = Substitute.For<IFormDefinitionRepository>();
    private readonly IFormRecordRepository _records = Substitute.For<IFormRecordRepository>();
    private readonly IFormRecordIndexRepository _indexes = Substitute.For<IFormRecordIndexRepository>();
    private readonly IDistributedEventBus _events = Substitute.For<IDistributedEventBus>();
    private readonly PublicFormAppService _service;

    public PublicFormAppServiceTests()
    {
        _indexes.DeleteByFormRecordIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(_tenantId);
        var manager = new FormRecordManager();
        Wire(manager);
        _service = new PublicFormAppService(
            _definitions,
            _records,
            manager,
            new FormRecordValuesValidator(),
            new FormRecordIndexService(_indexes, SimpleGuidGenerator.Instance),
            _events);
        Wire(_service, tenant);
    }

    [Fact]
    public async Task PublicForm_PublishedActive_Anonymous_ReturnsDefinition()
    {
        var definition = Published("intake");
        _definitions.FindByKeyAsync("intake", true, Arg.Any<CancellationToken>()).Returns(definition);

        var dto = await _service.GetPublishedByKeyAsync("intake");

        dto.ShouldNotBeNull();
        dto!.Key.ShouldBe("intake");
        dto.Fields.ShouldContain(field => field.Name == "message");
    }

    [Fact]
    public async Task PublicForm_Draft_ReturnsNull()
    {
        _definitions.FindByKeyAsync("intake", true, Arg.Any<CancellationToken>()).Returns(Draft("intake"));

        (await _service.GetPublishedByKeyAsync("intake")).ShouldBeNull();
    }

    [Fact]
    public async Task PublicForm_Inactive_ReturnsNull()
    {
        var definition = Published("intake");
        definition.SetActive(false);
        _definitions.FindByKeyAsync("intake", true, Arg.Any<CancellationToken>()).Returns(definition);

        (await _service.GetPublishedByKeyAsync("intake")).ShouldBeNull();
    }

    [Fact]
    public async Task PublicSubmit_PublishedActive_Anonymous_CreatesRecord()
    {
        var definition = Published("intake");
        _definitions.FindByKeyAsync("intake", true, Arg.Any<CancellationToken>()).Returns(definition);
        FormRecord? saved = null;
        _records.InsertAsync(Arg.Any<FormRecord>(), true, Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                saved = call.Arg<FormRecord>();
                return saved;
            });

        var dto = await _service.SubmitAsync("intake", new PublicFormSubmitInput
        {
            Values = new Dictionary<string, object?> { ["message"] = "hello" }
        });

        saved.ShouldNotBeNull();
        saved!.TenantId.ShouldBe(_tenantId);
        saved.SourceModule.ShouldBe(PublicFormAppService.PublicSourceModule);
        saved.FormKey.ShouldBe("intake");
        dto.SourceModule.ShouldBe("cms");
        dto.Values["message"]!.ToString().ShouldBe("hello");
        await _events.Received(1).PublishAsync(Arg.Is<FormRecordCreatedEto>(eto =>
            eto.SourceModule == "cms" && eto.FormKey == "intake" && eto.TenantId == _tenantId));
    }

    [Fact]
    public async Task PublicSubmit_Draft_Rejected()
    {
        _definitions.FindByKeyAsync("intake", true, Arg.Any<CancellationToken>()).Returns(Draft("intake"));

        var error = await Should.ThrowAsync<BusinessException>(() => _service.SubmitAsync("intake", new PublicFormSubmitInput
        {
            Values = new Dictionary<string, object?> { ["message"] = "hello" }
        }));

        error.Code.ShouldBe(SufiFormsErrorCodes.FormDefinitionNotPublished);
        await _records.DidNotReceiveWithAnyArgs().InsertAsync(null!, false, CancellationToken.None);
    }

    [Fact]
    public async Task PublicSubmit_Inactive_Rejected()
    {
        var definition = Published("intake");
        definition.SetActive(false);
        _definitions.FindByKeyAsync("intake", true, Arg.Any<CancellationToken>()).Returns(definition);

        var error = await Should.ThrowAsync<BusinessException>(() => _service.SubmitAsync("intake", new PublicFormSubmitInput
        {
            Values = new Dictionary<string, object?> { ["message"] = "hello" }
        }));

        error.Code.ShouldBe(SufiFormsErrorCodes.FormDefinitionNotPublished);
    }

    [Fact]
    public void Admin_form_services_stay_permissioned_and_public_submit_is_anonymous()
    {
        typeof(FormDefinitionAppService).GetCustomAttribute<AuthorizeAttribute>()!.Policy
            .ShouldBe(SufiFormsPermissions.Forms.Default);
        typeof(FormRecordAppService).GetCustomAttribute<AuthorizeAttribute>()!.Policy
            .ShouldBe(SufiFormsPermissions.Records.Default);
        typeof(FormRecordAppService).GetMethod(nameof(FormRecordAppService.CreateAsync))!
            .GetCustomAttribute<AuthorizeAttribute>()!.Policy
            .ShouldBe(SufiFormsPermissions.Records.Create);
        typeof(PublicFormAppService).GetCustomAttribute<AllowAnonymousAttribute>().ShouldNotBeNull();
    }

    private static FormDefinition Draft(string key)
    {
        var definition = new FormDefinition(Guid.NewGuid(), null, key, key, key);
        definition.AddField(new FormField(Guid.NewGuid(), "message", FormFieldType.Text, "Message"));
        return definition;
    }

    private static FormDefinition Published(string key)
    {
        var definition = Draft(key);
        definition.Publish(incrementVersion: false);
        return definition;
    }

    private static void Wire(object target, ICurrentTenant? tenant = null)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IGuidGenerator>().Returns(SimpleGuidGenerator.Instance);
        lazy.LazyGetService<IGuidGenerator>(Arg.Any<IGuidGenerator>()).Returns(SimpleGuidGenerator.Instance);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 11, 28, 0, 0, 0, DateTimeKind.Utc));
        lazy.LazyGetRequiredService<IClock>().Returns(clock);
        if (tenant != null)
        {
            lazy.LazyGetRequiredService<ICurrentTenant>().Returns(tenant);
        }

        lazy.LazyGetRequiredService<ICurrentUser>().Returns(Substitute.For<ICurrentUser>());
        target.GetType().GetProperty("LazyServiceProvider", BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(target, lazy);
    }
}
