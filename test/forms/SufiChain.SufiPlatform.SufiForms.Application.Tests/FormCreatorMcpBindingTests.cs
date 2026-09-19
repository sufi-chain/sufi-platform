using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Internal;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiForms.Copilots;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Forms.Dtos;
using SufiChain.SufiPlatform.SufiForms.Mcp;
using Xunit;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormCreatorMcpBindingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Definition_failure_preserves_business_code_without_staging(bool confirmed)
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        using var services = new ServiceCollection().AddSingleton(new FormDesignerMcpAppService(forms)).BuildServiceProvider();
        var method = typeof(FormDesignerMcpAppService).GetMethod(nameof(FormDesignerMcpAppService.CreateDefinitionAsync))!;
        var tool = new InternalMCPTool("forms.create_definition", "Build draft", "{}",
            typeof(FormDesignerMcpAppService), method, services);
        using var scope = confirmed ? new FormCreatorBuildScope(null) : null;
        using var input = JsonDocument.Parse(confirmed
            ? InputJson.Replace("\\\"placeholder\\\"", "\\\"unsupportedOption\\\"")
            : InputJson);

        var result = await tool.ExecuteAsync(new WorkspaceContext(), new() { ["input"] = input.RootElement });

        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(confirmed
            ? "SufiForms:CreatorInvalidDefinition"
            : "SufiForms:CreatorConfirmationRequired");
        scope?.Definition.ShouldBeNull();
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
    }

    [Fact]
    public async Task Validation_failure_identifies_members_for_tool_argument_repair()
    {
        using var services = new ServiceCollection().AddSingleton<InvalidInputService>().BuildServiceProvider();
        var method = typeof(InvalidInputService).GetMethod(nameof(InvalidInputService.InvokeAsync))!;
        var tool = new InternalMCPTool("validation-test", "Validate", "{}", typeof(InvalidInputService), method, services);
        var result = await tool.ExecuteAsync(new WorkspaceContext(), new());
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("input.Name");
        result.ErrorMessage.ShouldContain("Name is required");
    }

    public sealed class InvalidInputService
    {
        public Task InvokeAsync() => Task.FromException(new AbpValidationException("Invalid arguments",
            new List<ValidationResult> { new("Name is required", new[] { "input.Name" }) }));
    }

    private const string InputJson = """
        {"key":"registration","name":"Registration","displayName":"Registration",
         "fields":[{"name":"email","displayName":"Email","fieldType":60,"isRequired":true,
                     "optionsJson":"{\"placeholder\":\"you@example.com\"}"}]}
        """;

    [Fact]
    public void Generated_schema_describes_nested_fields_required_names_and_enum_values()
    {
        var method = typeof(FormDesignerMcpAppService).GetMethod(nameof(FormDesignerMcpAppService.CreateDefinitionAsync))!;
        using var document = JsonDocument.Parse(new JsonSchemaGenerator().GenerateSchema(method));
        var input = document.RootElement.GetProperty("properties").GetProperty("input");
        input.GetProperty("required").EnumerateArray().Select(x => x.GetString()).ShouldContain("displayName");
        var fields = input.GetProperty("properties").GetProperty("fields");
        fields.GetProperty("type").GetString().ShouldBe("array");
        var field = fields.GetProperty("items");
        field.GetProperty("type").GetString().ShouldBe("object");
        field.GetProperty("required").EnumerateArray().Select(x => x.GetString()).ShouldContain("name");
        var type = field.GetProperty("properties").GetProperty("fieldType");
        type.GetProperty("type").GetString().ShouldBe("integer");
        type.GetProperty("enum").EnumerateArray().Select(x => x.GetInt32()).ShouldContain((int)FormFieldType.Email);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Camel_case_tool_input_binds_required_properties_and_numeric_or_named_enums(bool namedEnum)
    {
        using var document = JsonDocument.Parse(namedEnum ? InputJson.Replace("\"fieldType\":60", "\"fieldType\":\"Email\"") : InputJson);
        var method = typeof(FormDesignerMcpAppService).GetMethod(nameof(FormDesignerMcpAppService.CreateDefinitionAsync))!;
        var args = new MethodParameterBinder().BindParameters(method, new() { ["input"] = document.RootElement });
        var input = args[0].ShouldBeOfType<CreateUpdateFormDefinitionDto>();
        Validator.ValidateObject(input, new ValidationContext(input), true);
        Validator.ValidateObject(input.Fields[0], new ValidationContext(input.Fields[0]), true);
        input.Key.ShouldBe("registration");
        input.DisplayName.ShouldBe("Registration");
        input.Fields[0].FieldType.ShouldBe(FormFieldType.Email);
        input.Fields[0].IsRequired.ShouldBeTrue();
    }

    [Fact]
    public async Task Real_internal_tool_pipeline_stages_camel_case_input_after_confirmation()
    {
        var forms = Substitute.For<IFormDefinitionAppService>();
        using var services = new ServiceCollection().AddSingleton(new FormDesignerMcpAppService(forms)).BuildServiceProvider();
        var method = typeof(FormDesignerMcpAppService).GetMethod(nameof(FormDesignerMcpAppService.CreateDefinitionAsync))!;
        var tool = new InternalMCPTool("forms.create_definition", "Build draft",
            new JsonSchemaGenerator().GenerateSchema(method), typeof(FormDesignerMcpAppService), method, services);
        using var scope = new FormCreatorBuildScope(null);
        using var input = JsonDocument.Parse(InputJson);
        var result = await tool.ExecuteAsync(new WorkspaceContext(), new() { ["input"] = input.RootElement });
        result.Success.ShouldBeTrue(result.ErrorMessage);
        scope.Definition.ShouldNotBeNull();
        scope.Definition!.Fields[0].Name.ShouldBe("email");
        await forms.DidNotReceiveWithAnyArgs().CreateAsync(default!);
    }
}
