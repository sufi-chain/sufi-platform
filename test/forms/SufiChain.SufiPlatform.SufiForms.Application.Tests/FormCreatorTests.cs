using System.Text.Json;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using SufiChain.SufiPlatform.SufiForms.Copilots;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Forms.Dtos;
using SufiChain.SufiPlatform.SufiForms.Mcp;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormCreatorTests
{
    private static CreateUpdateFormDefinitionDto Definition() => new()
    {
        Key = "registration", Name = "Registration", DisplayName = "Registration",
        Fields = [new() { Name = "email", DisplayName = "Email", FieldType = FormFieldType.Email, IsRequired = true }]
    };

    [Fact]
    public async Task Tool_cannot_build_from_chat_confirmation_or_without_server_scope()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var tool = new FormDesignerMcpAppService(forms);
        await Should.ThrowAsync<BusinessException>(() => tool.CreateDefinitionAsync(Definition()));
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
    }

    [Fact]
    public async Task Scope_allows_one_matching_operation_and_never_persists_inside_tool()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var tool = new FormDesignerMcpAppService(forms);
        using (var editScope = new FormCreatorBuildScope(Guid.NewGuid()))
            await Should.ThrowAsync<BusinessException>(() => tool.CreateDefinitionAsync(Definition()));
        using (var scope = new FormCreatorBuildScope(null))
        {
            var input = Definition();
            await tool.CreateDefinitionAsync(input);
            scope.Definition.ShouldBeSameAs(input);
            await Should.ThrowAsync<BusinessException>(() => tool.CreateDefinitionAsync(Definition()));
            await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
        }
        await Should.ThrowAsync<BusinessException>(() => tool.CreateDefinitionAsync(Definition()));
    }

    [Fact]
    public async Task Independent_execution_contexts_do_not_share_build_capabilities()
    {
        async Task<string> Build(string key)
        {
            using var scope = new FormCreatorBuildScope(null);
            await Task.Yield();
            var input = Definition();
            input.Key = key;
            await new FormDesignerMcpAppService(Substitute.For<IFormDefinitionAppService>()).CreateDefinitionAsync(input);
            return scope.Definition!.Key;
        }
        (await Task.WhenAll(Build("one"), Build("two"))).ShouldBe(new[] { "one", "two" });
        Should.Throw<BusinessException>(() => FormCreatorBuildScope.Stage(null, Definition()));
    }

    [Theory]
    [InlineData("{broken")]
    [InlineData("[]")]
    [InlineData("{\"unknownFeature\":true}")]
    [InlineData("{\"min\":10,\"max\":1}")]
    [InlineData("{\"options\":null}")]
    public async Task Invalid_options_never_stage_a_definition(string options)
    {
        using var scope = new FormCreatorBuildScope(null);
        var input = Definition();
        input.Fields[0].OptionsJson = options;
        await Should.ThrowAsync<BusinessException>(() =>
            new FormDesignerMcpAppService(Substitute.For<IFormDefinitionAppService>()).CreateDefinitionAsync(input));
        scope.Definition.ShouldBeNull();
    }

    [Fact]
    public async Task Catalog_covers_every_enum_type_and_actual_indexing_rules()
    {
        var catalog = await new FormDesignerMcpAppService(Substitute.For<IFormDefinitionAppService>()).GetDesignerCatalogAsync();
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(catalog));
        var types = json.RootElement.GetProperty("FieldTypes").EnumerateArray().ToArray();
        types.Length.ShouldBe(Enum.GetValues<FormFieldType>().Length);
        foreach (var row in types)
            row.GetProperty("IsIndexable").GetBoolean().ShouldBe(FormField.IsIndexableType((FormFieldType)row.GetProperty("Value").GetInt32()));
    }

    [Fact]
    public async Task Provider_failure_after_tool_staging_does_not_save_a_partial_form()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(async _ =>
            {
                await new FormDesignerMcpAppService(forms).CreateDefinitionAsync(Definition());
                return await Task.FromException<CopilotRuntimeResultDto>(new InvalidOperationException("provider failed after tool"));
            });
        var service = new AuthorizedCreator(runtime, forms);
        await Should.ThrowAsync<InvalidOperationException>(() => service.BuildAsync(new() { Confirmed = true, Proposal = "Approved requirements" }));
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
    }

    [Fact]
    public async Task Build_uses_current_proposal_and_returns_persisted_identity_not_model_claims()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        var saved = new FormDefinitionDto { Id = Guid.NewGuid(), Key = "registration" };
        forms.CreateAsync(Arg.Any<CreateUpdateFormDefinitionDto>()).Returns(saved);
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                call.Arg<CopilotRuntimeRequestDto>().MetadataJson.ShouldContain("Manually revised proposal");
                await new FormDesignerMcpAppService(forms).CreateDefinitionAsync(Definition());
                return new CopilotRuntimeResultDto { Message = "a fabricated identity" };
            });
        var result = await new AuthorizedCreator(runtime, forms).BuildAsync(new() { Confirmed = true, Proposal = "Manually revised proposal" });
        result.ShouldBeSameAs(saved);
        await forms.Received(1).CreateAsync(Arg.Any<CreateUpdateFormDefinitionDto>());
    }

    [Fact]
    public async Task Confirmation_is_required_before_runtime_and_empty_tool_result_cannot_claim_success()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        var service = new AuthorizedCreator(runtime, forms);
        await Should.ThrowAsync<BusinessException>(() => service.BuildAsync(new() { Proposal = "Create it" }));
        await runtime.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CopilotRuntimeResultDto { Message = "Done" });
        await Should.ThrowAsync<BusinessException>(() => service.BuildAsync(new() { Proposal = "Create it", Confirmed = true }));
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
    }

    [Fact]
    public async Task Edit_preserves_existing_ids_and_rejects_published_forms()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var id = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var current = new FormDefinitionDto
        {
            Id = id, Key = "registration", PublishState = FormPublishState.Draft,
            Fields = [new() { Id = fieldId, Name = "email", FieldType = FormFieldType.Email }]
        };
        forms.GetAsync(id).Returns(current);
        var input = Definition();
        input.Fields[0].Id = fieldId;
        using (var scope = new FormCreatorBuildScope(id))
        {
            await new FormDesignerMcpAppService(forms).UpdateDefinitionAsync(id, input);
            scope.Definition!.Fields[0].Id.ShouldBe(fieldId);
        }
        current.PublishState = FormPublishState.Published;
        using var deniedScope = new FormCreatorBuildScope(id);
        await Should.ThrowAsync<BusinessException>(() => new FormDesignerMcpAppService(forms).UpdateDefinitionAsync(id, input));
        deniedScope.Definition.ShouldBeNull();
    }

    // These tests isolate orchestration and tool boundaries. Host permission interception needs integration validation.
    private sealed class AuthorizedCreator(ICopilotRuntimeAppService runtime, IFormDefinitionAppService forms)
        : FormCreatorAppService(Substitute.For<IPlatformCopilotResolver>(), runtime, forms)
    {
        protected override Task CheckOperationAsync(Guid? id) => Task.CompletedTask;
    }
}
