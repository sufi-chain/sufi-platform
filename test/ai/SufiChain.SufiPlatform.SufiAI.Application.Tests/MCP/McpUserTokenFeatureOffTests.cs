using System;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Controllers;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.MCP.Auth;
using Volo.Abp.Authorization;
using Volo.Abp.Features;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.MCP;

/// <summary>
/// POST /api/ai/mcp/user-token/verify is anonymous. The token is the credential only when MCP is on.
/// </summary>
public class McpUserTokenFeatureOffTests
{
    [Fact]
    public async Task Verify_throws_when_mcp_is_off_and_does_not_read_the_token()
    {
        var tokens = Substitute.For<IMcpUserTokenService>();
        var controller = new McpUserTokenController(tokens, Checker(mcpEnabled: false));

        var exception = await Should.ThrowAsync<AbpAuthorizationException>(() =>
            controller.VerifyAsync(new VerifyMcpUserTokenInput
            {
                ServerId = Guid.NewGuid(),
                Token = "presented-token"
            }));

        exception.Code.ShouldNotBeNull();
        exception.Code.ShouldStartWith("Volo.Feature:");
        tokens.DidNotReceive().Validate(Arg.Any<string?>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task Verify_throws_when_the_ai_master_feature_is_off()
    {
        var tokens = Substitute.For<IMcpUserTokenService>();
        var controller = new McpUserTokenController(tokens, Checker(mcpEnabled: true, aiEnabled: false));

        var exception = await Should.ThrowAsync<AbpAuthorizationException>(() =>
            controller.VerifyAsync(new VerifyMcpUserTokenInput
            {
                ServerId = Guid.NewGuid(),
                Token = "presented-token"
            }));

        exception.Code.ShouldStartWith("Volo.Feature:");
        tokens.DidNotReceive().Validate(Arg.Any<string?>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task Verify_still_rejects_an_unknown_token_when_mcp_is_on()
    {
        var tokens = Substitute.For<IMcpUserTokenService>();
        tokens.Validate(Arg.Any<string?>(), Arg.Any<Guid>()).Returns((McpUserTokenPayload?)null);
        var controller = new McpUserTokenController(tokens, Checker(mcpEnabled: true));

        var result = await controller.VerifyAsync(new VerifyMcpUserTokenInput
        {
            ServerId = Guid.NewGuid(),
            Token = "presented-token"
        });

        result.IsValid.ShouldBeFalse();
        result.UserId.ShouldBeNull();
    }

    private static IFeatureChecker Checker(bool mcpEnabled, bool aiEnabled = true)
    {
        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(Arg.Any<string>()).Returns(call =>
        {
            var name = (string)call[0];
            var enabled = name switch
            {
                SufiAIFeatures.Enable => aiEnabled,
                SufiAIFeatures.MCP => mcpEnabled,
                _ => true
            };
            return Task.FromResult(enabled);
        });
        return features;
    }
}
