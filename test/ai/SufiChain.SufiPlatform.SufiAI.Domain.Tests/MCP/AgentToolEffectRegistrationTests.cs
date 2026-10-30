using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Execution;
using SufiChain.SufiPlatform.SufiAI.MCP.Internal;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

/// <summary>
/// sufi-chain#312 / #314: tools declare an effect, and the registrar refuses a tool that breaks the rules.
/// </summary>
public class AgentToolEffectRegistrationTests
{
    [Fact]
    public void Unmarked_tools_resolve_to_write_and_an_explicit_read_effect_is_read()
    {
        var unmarked = new SufiAiMcpToolAttribute("demo.unmarked", "x");
        unmarked.TryGetEffect(out var write, out var writeRefusal).ShouldBeTrue();
        writeRefusal.ShouldBeNull();
        write.ShouldBe(AgentActionEffect.Write);

        var readOnly = new SufiAiMcpToolAttribute("demo.read", "x") { ReadOnly = true };
        readOnly.TryGetEffect(out var fromFlag, out _).ShouldBeTrue();
        fromFlag.ShouldBe(AgentActionEffect.Read);

        var effect = new SufiAiMcpToolAttribute("demo.effect", "x") { Effect = AgentActionEffect.Read };
        effect.TryGetEffect(out var fromEffect, out _).ShouldBeTrue();
        fromEffect.ShouldBe(AgentActionEffect.Read);

        var service = typeof(Marked);
        ReflectionToolDiscoveryService.IsReadOnly(service.GetMethod(nameof(Marked.Read))!).ShouldBeTrue();
        ReflectionToolDiscoveryService.IsReadOnly(service.GetMethod(nameof(Marked.EffectRead))!).ShouldBeTrue();
        ReflectionToolDiscoveryService.IsReadOnly(service.GetMethod(nameof(Marked.Unmarked))!).ShouldBeFalse();
        ReflectionToolDiscoveryService.AllowsAnonymous(service.GetMethod(nameof(Marked.PublicEffectRead))!).ShouldBeTrue();
        ReflectionToolDiscoveryService.AllowsAnonymous(service.GetMethod(nameof(Marked.PublicWrite))!).ShouldBeFalse();
    }

    [Fact]
    public void An_undefined_or_unspecified_effect_is_refused_at_registration()
    {
        var undefined = new SufiAiMcpToolAttribute("demo.bad", "x") { Effect = (AgentActionEffect)99 };
        undefined.TryGetEffect(out _, out var refusal).ShouldBeFalse();
        refusal.ShouldBe("The tool effect is not Read or Write.");

        MCPKernelToolRegistrar.GetRegistrationRefusal(Tool("demo.bad", (AgentActionEffect)99))
            .ShouldBe("The tool effect is not Read or Write.");
        MCPKernelToolRegistrar.GetRegistrationRefusal(Tool("demo.unset", AgentActionEffect.Unspecified))
            .ShouldBe("The tool effect is not Read or Write.");
    }

    [Fact]
    public void A_read_only_tool_cannot_also_declare_a_write_effect()
    {
        var conflict = new SufiAiMcpToolAttribute("demo.conflict", "x")
        {
            ReadOnly = true,
            Effect = AgentActionEffect.Write
        };

        conflict.TryGetEffect(out _, out var refusal).ShouldBeFalse();
        refusal.ShouldNotBeNull();
        ReflectionToolDiscoveryService.IsReadOnly(typeof(Marked).GetMethod(nameof(Marked.Conflict))!).ShouldBeFalse();
        ReflectionToolDiscoveryService.AllowsAnonymous(typeof(Marked).GetMethod(nameof(Marked.Conflict))!).ShouldBeFalse();
    }

    [Fact]
    public async Task Registrar_stores_the_effect_and_refuses_tools_that_break_the_rules()
    {
        var read = Tool("demo.read", AgentActionEffect.Read);
        var write = Tool("demo.write", AgentActionEffect.Write);
        var blank = Tool("  ", AgentActionEffect.Read);
        var undeclared = Substitute.For<IMCPTool>();
        undeclared.Name.Returns("demo.undeclared");
        undeclared.Description.Returns("x");
        undeclared.ParameterSchema.Returns("{}");
        var duplicate = Tool("demo.read", AgentActionEffect.Write);
        var collapsed = Tool("demo_read", AgentActionEffect.Write);

        var tools = new List<IMCPTool> { read, write, blank, undeclared, duplicate, collapsed };
        var registry = Substitute.For<IMCPToolRegistry>();
        registry.ResolveAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new MCPToolResolutionResult { Tools = tools });

        var registrar = new MCPKernelToolRegistrar(
            registry,
            Array.Empty<IMcpToolDecisionGate>(),
            NullLogger<MCPKernelToolRegistrar>.Instance);
        var userId = Guid.Parse("aaaaaaaa-0000-0000-0000-00000000000a");
        var kernel = new Kernel();
        await registrar.RegisterToolsAsync(
            kernel,
            new WorkspaceContext
            {
                WorkspaceName = "demo",
                CallContext = AgentCallContext.ForUser(null, userId, new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(AbpClaimTypes.UserId, userId.ToString())
                }, "test")))
            },
            tools.Select(tool => tool.Name).ToList());

        kernel.Plugins.Select(plugin => plugin.Name).OrderBy(name => name).ShouldBe(new[]
        {
            "MCP_demo_read",
            "MCP_demo_write"
        });

        var readFunction = kernel.Plugins.Single(plugin => plugin.Name == "MCP_demo_read").Single();
        readFunction.Metadata.AdditionalProperties[MCPKernelToolRegistrar.EffectMetadataKey]
            .ShouldBe(AgentActionEffect.Read);
        kernel.Plugins.Single(plugin => plugin.Name == "MCP_demo_write").Single()
            .Metadata.AdditionalProperties[MCPKernelToolRegistrar.EffectMetadataKey]
            .ShouldBe(AgentActionEffect.Write);

        MCPKernelToolRegistrar.GetRegistrationRefusal(blank).ShouldBe("A tool name is required.");
        MCPKernelToolRegistrar.GetRegistrationRefusal(undeclared).ShouldBe("A tool must declare an effect.");
    }

    private static DeclaredTool Tool(string name, AgentActionEffect effect) => new(name, effect);

    private sealed class DeclaredTool : IMCPTool, IMCPToolEffect
    {
        public DeclaredTool(string name, AgentActionEffect effect)
        {
            Name = name;
            Effect = effect;
        }

        public string Name { get; }
        public string Description => "x";
        public string ParameterSchema => "{}";
        public MCPToolType ToolType => MCPToolType.Internal;
        public string Source => "test";
        public AgentActionEffect Effect { get; }
        public bool IsReadOnly => Effect == AgentActionEffect.Read;

        public Task<MCPToolExecutionResult> ExecuteAsync(
            WorkspaceContext context,
            Dictionary<string, object?> parameters,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(MCPToolExecutionResult.CreateSuccess("ok", 0));
    }

    public sealed class Marked
    {
        [SufiAiMcpTool("m.read", "x", ReadOnly = true)]
        public Task<string> Read() => Task.FromResult("ok");

        [SufiAiMcpTool("m.effect", "x", Effect = AgentActionEffect.Read)]
        public Task<string> EffectRead() => Task.FromResult("ok");

        [SufiAiMcpTool("m.unmarked", "x")]
        public Task<string> Unmarked() => Task.FromResult("ok");

        [SufiAiMcpTool("m.public_effect", "x", Public = true, Effect = AgentActionEffect.Read)]
        public Task<string> PublicEffectRead() => Task.FromResult("ok");

        [SufiAiMcpTool("m.public_write", "x", Public = true)]
        public Task<string> PublicWrite() => Task.FromResult("ok");

        [SufiAiMcpTool("m.conflict", "x", ReadOnly = true, Effect = AgentActionEffect.Write)]
        public Task<string> Conflict() => Task.FromResult("ok");
    }
}
