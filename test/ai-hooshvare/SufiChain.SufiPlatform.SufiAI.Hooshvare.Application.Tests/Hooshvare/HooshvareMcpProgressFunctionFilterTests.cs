using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Execution;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareMcpProgressFunctionFilterTests
{
    [Theory]
    [InlineData("calendar.list_calendars")]
    [InlineData("calendar.get_current_time")]
    [InlineData("calendar.get_working_hours")]
    [InlineData("calendar.get_free_busy")]
    [InlineData("calendar.find_free_slots")]
    [InlineData("calendar.search_events")]
    [InlineData("calendar.create_event")]
    [InlineData("calendar.move_event")]
    [InlineData("calendar.move_occurrence")]
    [InlineData("calendar.cancel_event")]
    [InlineData("calendar.cancel_occurrence")]
    [InlineData("calendar.test_availability")]
    [InlineData("forms.list_forms")]
    [InlineData("forms.describe_form")]
    [InlineData("forms.query_records")]
    [InlineData("forms.get_record")]
    [InlineData("forms.create_record")]
    [InlineData("forms.update_record")]
    [InlineData("forms.get_designer_catalog")]
    [InlineData("forms.get_definition")]
    [InlineData("forms.create_definition")]
    [InlineData("forms.update_definition")]
    [InlineData("contacts.send_onboarding_otp")]
    [InlineData("contacts.verify_onboarding_otp")]
    [InlineData("contacts.register_onboarded_user")]
    [InlineData("contacts.complete_onboarding")]
    [InlineData("helpdesk.kb.search_project_guidance")]
    [InlineData("cms.get_designer_catalog")]
    public async Task Registered_Tool_Progress_Should_Resolve_Specific_Activity_In_Every_Culture(string toolName)
    {
        var (kernel, function, progress) = await RegisterToolAsync(toolName);

        // Invoke the plugin's cloned function through the real registrar/filter pipeline.
        await kernel.InvokeAsync(function);

        progress.Count.ShouldBe(2);
        progress[0].Stage.ShouldBe(HooshvareTurnProgressStages.CallingTool);
        progress[0].Status.ShouldBe(HooshvareTurnProgressStatuses.Started);
        progress[1].Stage.ShouldBe(HooshvareTurnProgressStages.ToolResult);
        progress[1].Status.ShouldBe(HooshvareTurnProgressStatuses.Succeeded);
        foreach (var step in progress)
        {
            step.ToolName.ShouldBe(toolName);
            foreach (var culture in new[] { "en", "fa", "ar", "es" })
            {
                using var stream = typeof(HooshvareMcpProgressFunctionFilterTests).Assembly
                    .GetManifestResourceStream($"ChatPublic.{culture}.json");
                stream.ShouldNotBeNull();
                using var document = JsonDocument.Parse(stream!);
                var texts = document.RootElement.GetProperty("texts");
                var key = "Messenger:AiActivity:" + step.ToolName;
                texts.TryGetProperty(key, out var activity).ShouldBeTrue($"Missing {culture}: {key}");
                activity.GetString().ShouldNotBeNullOrWhiteSpace();
                activity.GetString().ShouldNotBe(key);
                activity.GetString().ShouldNotBe(texts.GetProperty("Messenger:AiProgressLookingUp").GetString());
            }
        }
    }

    [Fact]
    public async Task Failed_Tool_Progress_Should_Keep_Original_Name()
    {
        var (kernel, function, progress) = await RegisterToolAsync("calendar.search_events", fails: true);

        await Should.ThrowAsync<InvalidOperationException>(() => kernel.InvokeAsync(function));

        progress.Count.ShouldBe(2);
        progress[1].Stage.ShouldBe(HooshvareTurnProgressStages.ToolResult);
        progress[1].Status.ShouldBe(HooshvareTurnProgressStatuses.Failed);
        progress.ShouldAllBe(step => step.ToolName == "calendar.search_events");
    }

    [Fact]
    public async Task External_Tool_Should_Keep_Qualified_Registry_Name()
    {
        const string toolName = "external.support-server.search_articles";
        var (kernel, function, progress) = await RegisterToolAsync(toolName);

        await kernel.InvokeAsync(function);

        progress.Count.ShouldBe(2);
        progress.ShouldAllBe(step => step.ToolName == toolName);
    }

    [Theory]
    [InlineData(null, "lookup")]
    [InlineData("Custom", "Custom.lookup")]
    public async Task Non_Mcp_Function_Should_Keep_Existing_Name_Fallback(string? pluginName, string expectedName)
    {
        var kernel = Kernel.CreateBuilder().Build();
        var progress = AttachReporter(kernel);
        var function = KernelFunctionFactory.CreateFromMethod(() => "ok", functionName: "lookup");
        if (pluginName is not null)
        {
            function = kernel.Plugins.AddFromFunctions(pluginName, new[] { function })["lookup"];
        }

        await kernel.InvokeAsync(function);

        progress.Count.ShouldBe(2);
        progress.ShouldAllBe(step => step.ToolName == expectedName);
    }

    private static async Task<(Kernel Kernel, KernelFunction Function, List<HooshvareTurnProgressDto> Progress)>
        RegisterToolAsync(string toolName, bool fails = false)
    {
        var tool = Substitute.For<IMCPTool>();
        tool.Name.Returns(toolName);
        tool.Description.Returns("Test tool");
        tool.ParameterSchema.Returns("{\"type\":\"object\",\"properties\":{}}");
        tool.ExecuteAsync(Arg.Any<WorkspaceContext>(), Arg.Any<Dictionary<string, object?>>(),
                Arg.Any<CancellationToken>())
            .Returns(_ => fails
                ? Task.FromException<MCPToolExecutionResult>(new InvalidOperationException("Tool failed"))
                : Task.FromResult(MCPToolExecutionResult.CreateSuccess("ok", 0)));
        var registry = Substitute.For<IMCPToolRegistry>();
        registry.ResolveAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new MCPToolResolutionResult { Tools = [tool] });
        var kernel = Kernel.CreateBuilder().Build();
        var progress = AttachReporter(kernel);
        var registrar = new MCPKernelToolRegistrar(registry, NullLogger<MCPKernelToolRegistrar>.Instance);

        await registrar.RegisterToolsAsync(kernel, new WorkspaceContext { WorkspaceName = "test" }, [toolName]);

        return (kernel, kernel.Plugins.Single().Single(), progress);
    }

    private static List<HooshvareTurnProgressDto> AttachReporter(Kernel kernel)
    {
        var progress = new List<HooshvareTurnProgressDto>();
        var reporter = Substitute.For<IHooshvareTurnProgressReporter>();
        reporter.ReportAsync(Arg.Any<HooshvareTurnProgressDto>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                progress.Add(call.Arg<HooshvareTurnProgressDto>());
                return Task.CompletedTask;
            });
        kernel.FunctionInvocationFilters.Add(new HooshvareMcpProgressFunctionFilter(reporter));
        return progress;
    }
}
