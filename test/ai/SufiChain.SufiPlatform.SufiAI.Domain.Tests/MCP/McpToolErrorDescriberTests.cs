using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Execution;
using SufiChain.SufiPlatform.SufiAI.MCP.Internal;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

/// <summary>
/// sufi-chain#340: a module business error reached the model as a bare code (CMS:PageDraftNotEmpty), so the model
/// gave up with a "technical problem". A registered describer now adds what to do; the code stays first.
/// </summary>
public class McpToolErrorDescriberTests
{
    private static readonly Guid Tenant = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000340");
    private static readonly Guid User = Guid.Parse("00000000-0000-0000-0000-000000000340");

    [Fact]
    public async Task Business_error_without_a_describer_returns_the_bare_code()
    {
        (await RunAsync()).ShouldBe("Demo:Refused");
    }

    [Fact]
    public async Task Describer_text_follows_the_code_and_gets_the_exception_data()
    {
        (await RunAsync(new DemoDescriber())).ShouldBe("Demo:Refused: refused inline style; use a class");
    }

    [Fact]
    public async Task A_throwing_or_silent_describer_falls_through_to_the_next_one_or_the_code()
    {
        (await RunAsync(new ThrowingDescriber(), new SilentDescriber())).ShouldBe("Demo:Refused");
        (await RunAsync(new ThrowingDescriber(), new DemoDescriber())).ShouldBe("Demo:Refused: refused inline style; use a class");
    }

    private static async Task<string?> RunAsync(params IMcpToolErrorDescriber[] describers)
    {
        var services = new ServiceCollection()
            .AddSingleton<ICurrentTenantAccessor>(AsyncLocalCurrentTenantAccessor.Instance)
            .AddSingleton<ICurrentTenant, CurrentTenant>()
            .AddSingleton<ICurrentPrincipalAccessor, ThreadCurrentPrincipalAccessor>()
            .AddScoped<FailingService>();
        foreach (var describer in describers)
        {
            services.AddSingleton(describer);
        }

        using var provider = services.BuildServiceProvider();
        var tool = new InternalMCPTool("demo.fail", "Fail", "{}", typeof(FailingService),
            typeof(FailingService).GetMethod(nameof(FailingService.Run))!, provider);
        var call = AgentCallContext.ForUser(Tenant, User, Principal());

        var result = await tool.ExecuteAsync(new WorkspaceContext { CallContext = call }, new());

        result.Success.ShouldBeFalse();
        return result.ErrorMessage;
    }

    private static ClaimsPrincipal Principal() =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, User.ToString()),
            new Claim(AbpClaimTypes.TenantId, Tenant.ToString())
        }, "test"));

    public sealed class FailingService
    {
        public async Task<string> Run()
        {
            await Task.Yield();
            throw new BusinessException("Demo:Refused").WithData("what", "inline style");
        }
    }

    private sealed class DemoDescriber : IMcpToolErrorDescriber
    {
        public string? Describe(string code, IReadOnlyDictionary<string, object?> data) =>
            code == "Demo:Refused" ? "refused " + data["what"] + "; use a class" : null;
    }

    private sealed class SilentDescriber : IMcpToolErrorDescriber
    {
        public string? Describe(string code, IReadOnlyDictionary<string, object?> data) => null;
    }

    private sealed class ThrowingDescriber : IMcpToolErrorDescriber
    {
        public string? Describe(string code, IReadOnlyDictionary<string, object?> data) =>
            throw new InvalidOperationException("broken describer");
    }
}
