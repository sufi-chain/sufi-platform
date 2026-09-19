using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;
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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Conversation_requires_the_reply_schema_in_discovery_and_proposal(bool prepareProposal)
    {
        var turnId = Guid.NewGuid();
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var schema = call.Arg<CopilotRuntimeRequestDto>().ResponseSchema;
            call.Arg<CopilotRuntimeRequestDto>().ProgressTurnId.ShouldBe(turnId);
            schema.ShouldNotBeNull();
            schema.Name.ShouldBe("form_creator_reply");
            using var json = JsonDocument.Parse(schema.SchemaJson);
            json.RootElement.GetProperty("required").EnumerateArray().Select(x => x.GetString())
                .ShouldBe(new[] { "message", "proposal" });
            json.RootElement.GetProperty("additionalProperties").GetBoolean().ShouldBeFalse();
            json.RootElement.GetProperty("properties").GetProperty("proposal").GetProperty("type")
                .EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { "string", "null" });
            return new CopilotRuntimeResultDto { Message = "{\"message\":\"Ready\",\"proposal\":null}", FinishReason = "stop" };
        });
        await new AuthorizedCreator(runtime, Substitute.For<IFormDefinitionAppService>())
            .ConverseAsync(new() { Message = "Design a form", PrepareProposal = prepareProposal, ProgressTurnId = turnId });
    }

    [Theory]
    [InlineData("length")]
    [InlineData("content_filter")]
    public async Task Incomplete_reply_is_rejected_even_when_it_contains_valid_json(string finishReason)
    {
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CopilotRuntimeResultDto
            {
                Message = "{\"message\":\"Ready\",\"proposal\":\"Partial proposal\"}", FinishReason = finishReason
            });
        var error = await Should.ThrowAsync<BusinessException>(() =>
            new AuthorizedCreator(runtime, Substitute.For<IFormDefinitionAppService>())
                .ConverseAsync(new() { Message = "Prepare proposal", PrepareProposal = true }));
        error.Code.ShouldBe("SufiForms:CreatorInvalidReply");
        await runtime.Received(1).SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("{\"message\":\"Ready\",\"proposal\":\"# Form\\n- Email\"}")]
    [InlineData(" \n```json\n{\"message\":\"Ready\",\"proposal\":\"# Form\\n- Email\"}\n``` \n")]
    [InlineData("```JSON\r\n{\"Message\":\"Ready\",\"Proposal\":\"# Form\\n- Email\"}\r\n```")]
    [InlineData("```\n{\"message\":\"Ready\",\"proposal\":\"# Form\\n- Email\"}\n```")]
    public async Task Complete_reply_envelopes_preserve_proposal_markdown(string response)
    {
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CopilotRuntimeResultDto { Message = response });
        var reply = await new AuthorizedCreator(runtime, Substitute.For<IFormDefinitionAppService>())
            .ConverseAsync(new() { Message = "Prepare proposal", PrepareProposal = true });
        reply.Message.ShouldBe("Ready");
        reply.Proposal.ShouldBe("# Form\n- Email");
        await runtime.Received(1).SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{\"proposal\":\"Unapproved form\"}")]
    [InlineData("{\"message\":\" \"}")]
    [InlineData("{\"message\":\"Ready\",\"proposal\":{\"fields\":[]}}")]
    [InlineData("```json\n{\"message\":\"Ready\"")]
    [InlineData("Here is the form: {\"message\":\"Ready\"}")]
    [InlineData("Which fields should the form contain?")]
    [InlineData("I am 64, Display: unrelated fragments")]
    [InlineData("```json\n{\"message\":\"One\"}\n{\"message\":\"Two\"}\n```")]
    [InlineData("```text\n{\"message\":\"Ready\"}\n```")]
    public async Task Invalid_reply_does_not_replay_tools_or_accept_a_proposal(string response)
    {
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        var forms = Substitute.For<IFormDefinitionAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CopilotRuntimeResultDto { Message = response });
        var error = await Should.ThrowAsync<BusinessException>(() => new AuthorizedCreator(runtime, forms)
            .ConverseAsync(new() { Message = "Prepare proposal", PrepareProposal = true }));
        error.Code.ShouldBe("SufiForms:CreatorInvalidReply");
        await runtime.Received(1).SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>());
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Reply_fields_cannot_exceed_the_next_request_limits(bool oversizedMessage)
    {
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CopilotRuntimeResultDto { Message = JsonSerializer.Serialize(new
            {
                message = oversizedMessage ? new string('x', 32001) : "Ready",
                proposal = oversizedMessage ? null : new string('x', 32001)
            }) });
        var error = await Should.ThrowAsync<BusinessException>(() =>
            new AuthorizedCreator(runtime, Substitute.For<IFormDefinitionAppService>())
                .ConverseAsync(new() { Message = "Prepare proposal" }));
        error.Code.ShouldBe("SufiForms:CreatorInvalidReply");
    }

    [Fact]
    public async Task Assistant_history_uses_the_json_reply_contract_without_inventing_a_proposal()
    {
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var history = call.Arg<CopilotRuntimeRequestDto>().ConversationHistory;
                history[0].Role.ShouldBe("user");
                history[0].Content.ShouldBe("Registration");
                history[1].Role.ShouldBe("assistant");
                using var envelope = JsonDocument.Parse(history[1].Content);
                envelope.RootElement.GetProperty("message").GetString().ShouldBe("Which fields?\nName or email?");
                envelope.RootElement.GetProperty("proposal").ValueKind.ShouldBe(JsonValueKind.Null);
                return new CopilotRuntimeResultDto { Message = "{\"message\":\"Ready\",\"proposal\":null}" };
            });
        var reply = await new AuthorizedCreator(runtime, Substitute.For<IFormDefinitionAppService>()).ConverseAsync(new()
        {
            Message = "Email", History =
            [
                new() { Content = "Registration" },
                new() { IsAssistant = true, Content = "Which fields?\nName or email?" }
            ]
        });
        reply.Proposal.ShouldBeNull();
    }

    [Fact]
    public async Task Conversation_forwards_the_selected_model_to_the_runtime_policy()
    {
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        var model = Guid.NewGuid();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CopilotRuntimeResultDto { Message = "{\"message\":\"Ready\"}" });
        await new AuthorizedCreator(runtime, Substitute.For<IFormDefinitionAppService>()).ConverseAsync(
            new() { Message = "Prepare proposal", ModelConfigurationId = model });
        await runtime.Received(1).SendAsync(
            Arg.Is<CopilotRuntimeRequestDto>(request => request.ModelConfigurationId == model), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Premature_response_is_reported_as_recoverable_without_replaying_the_turn()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CopilotRuntimeResultDto>(
                new HttpIOException(HttpRequestError.ResponseEnded, "response ended")));
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            new AuthorizedCreator(runtime, forms).ConverseAsync(new() { Message = "Prepare proposal", PrepareProposal = true }));
        exception.Code.ShouldBe("SufiForms:CreatorResponseInterrupted");
        await runtime.Received(1).SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>());
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
    }

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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_discards_supplied_field_ids_and_still_requires_confirmation(bool emptyId)
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var tool = new FormDesignerMcpAppService(forms);
        var input = Definition();
        var suppliedId = emptyId ? Guid.Empty : Guid.NewGuid();
        input.Fields[0].Id = suppliedId;
        input.Fields.Add(new() { Id = suppliedId, Name = "notes", DisplayName = "Notes", FieldType = FormFieldType.Text });

        using (var scope = new FormCreatorBuildScope(null))
        {
            await tool.CreateDefinitionAsync(input);
            scope.Definition.ShouldBeSameAs(input);
            scope.Definition!.Fields.ShouldAllBe(field => field.Id == null);
            scope.Definition.Fields.Select(field => field.Name).ShouldBe(new[] { "email", "notes" });
        }

        input.Fields[0].Id = suppliedId;
        var exception = await Should.ThrowAsync<BusinessException>(() => tool.CreateDefinitionAsync(input));
        exception.Code.ShouldBe("SufiForms:CreatorConfirmationRequired");
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
        await forms.DidNotReceiveWithAnyArgs().UpdateAsync(default, default!);
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

    [Theory]
    [InlineData("[]", "OptionsMustBeObject")]
    [InlineData("{\"privateUnknownProperty\":\"private-value\"}", "OptionsJsonMalformedOrUnsupportedPropertyOrValue")]
    [InlineData("{\"min\":10,\"max\":1}", "MinGreaterThanMax")]
    [InlineData("{\"rows\":0}", "RowsMustBePositive")]
    [InlineData("{\"options\":null}", "NullChoices")]
    public async Task Rejected_definition_logs_rule_and_field_index_without_form_content(string options, string rule)
    {
        using var scope = new FormCreatorBuildScope(null);
        var input = Definition();
        input.Fields[0].Name = "private-field-name";
        input.Fields[0].OptionsJson = options;
        var logger = Substitute.For<ILogger<FormDesignerMcpAppService>>();
        var tool = new FormDesignerMcpAppService(Substitute.For<IFormDefinitionAppService>(), logger);

        var exception = await Should.ThrowAsync<BusinessException>(() => tool.CreateDefinitionAsync(input));

        exception.Code.ShouldBe("SufiForms:CreatorInvalidDefinition");
        exception.Data["ValidationRule"].ShouldBe(rule);
        exception.Data["FieldIndex"].ShouldBe(0);
        scope.Definition.ShouldBeNull();
        var warnings = logger.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                           call.GetArguments()[0] is LogLevel.Warning)
            .Select(call => call.GetArguments()[2]?.ToString()).ToArray();
        warnings.ShouldContain(message => message != null && message.Contains(rule) && message.Contains("FieldIndex=0"));
        var messages = string.Join("\n", warnings);
        messages.ShouldNotContain("private-field-name");
        messages.ShouldNotContain("privateUnknownProperty");
        messages.ShouldNotContain("private-value");
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
        var model = Guid.NewGuid();
        var turnId = Guid.NewGuid();
        forms.CreateAsync(Arg.Any<CreateUpdateFormDefinitionDto>()).Returns(saved);
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                call.Arg<CopilotRuntimeRequestDto>().MetadataJson.ShouldContain("Manually revised proposal");
                call.Arg<CopilotRuntimeRequestDto>().ModelConfigurationId.ShouldBe(model);
                call.Arg<CopilotRuntimeRequestDto>().ProgressTurnId.ShouldBe(turnId);
                call.Arg<CopilotRuntimeRequestDto>().ResponseSchema.ShouldBeNull();
                await new FormDesignerMcpAppService(forms).CreateDefinitionAsync(Definition());
                return new CopilotRuntimeResultDto { Message = "a fabricated identity" };
            });
        var result = await new AuthorizedCreator(runtime, forms).BuildAsync(new()
            { Confirmed = true, Proposal = "Manually revised proposal", ModelConfigurationId = model, ProgressTurnId = turnId });
        result.ShouldBeSameAs(saved);
        await forms.Received(1).CreateAsync(Arg.Any<CreateUpdateFormDefinitionDto>());
    }

    [Theory]
    [InlineData("ended", false)]
    [InlineData("ended", true)]
    [InlineData("network", true)]
    [InlineData("timeout", true)]
    [InlineData("cancelled-timeout", true)]
    public async Task Interrupted_build_preserves_atomicity_and_does_not_replay(string failure, bool stage)
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        Exception cause = failure switch
        {
            "ended" => new HttpIOException(HttpRequestError.ResponseEnded, "incomplete body"),
            "network" => new HttpRequestException("connection lost"),
            "timeout" => new TimeoutException(),
            _ => new TaskCanceledException("request timeout", new TimeoutException())
        };
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(async _ =>
            {
                if (stage) await new FormDesignerMcpAppService(forms).CreateDefinitionAsync(Definition());
                return await Task.FromException<CopilotRuntimeResultDto>(cause);
            });
        var error = await Should.ThrowAsync<BusinessException>(() =>
            new AuthorizedCreator(runtime, forms).BuildAsync(new() { Confirmed = true, Proposal = "Approved" }));
        error.Code.ShouldBe(failure.Contains("timeout")
            ? "SufiForms:CreatorResponseTimedOut" : "SufiForms:CreatorResponseInterrupted");
        await runtime.Received(1).SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>());
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
        await forms.DidNotReceiveWithAnyArgs().UpdateAsync(default, default!);
        // The failed request must not leave ambient permission to invoke a build tool.
        await Should.ThrowAsync<BusinessException>(() => new FormDesignerMcpAppService(forms).CreateDefinitionAsync(Definition()));
    }

    [Fact]
    public async Task Caller_cancellation_is_not_misreported_as_provider_timeout()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        var runtime = Substitute.For<ICopilotRuntimeAppService>();
        runtime.SendAsync(Arg.Any<CopilotRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<CopilotRuntimeResultDto>(new CancellationToken(true)));
        await Should.ThrowAsync<OperationCanceledException>(() =>
            new AuthorizedCreator(runtime, forms).BuildAsync(new() { Confirmed = true, Proposal = "Approved" }));
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
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
