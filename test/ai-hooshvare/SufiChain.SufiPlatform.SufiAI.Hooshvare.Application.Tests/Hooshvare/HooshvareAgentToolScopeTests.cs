using System;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Application.Tests.Hooshvare;

/// <summary>sufi-chain#384 slice 5: a Hooshvare's level is stored in ExtraProperties (no schema change).</summary>
public class HooshvareAgentToolScopeTests
{
    [Fact]
    public void A_hooshvare_without_a_level_acts_with_the_users_own_permissions()
    {
        var definition = Create();
        definition.GetAgentToolScope().ShouldBeNull();
        HooshvareAgentToolScopes.Resolve(definition).ShouldBeNull();
    }

    [Theory]
    [InlineData("ReadOnly", AgentToolScope.ReadOnly)]
    [InlineData("readonly", AgentToolScope.ReadOnly)]
    [InlineData("Full", AgentToolScope.Full)]
    public void A_stored_level_is_applied(string value, AgentToolScope expected)
    {
        var definition = Create();
        definition.SetAgentToolScope(value);
        HooshvareAgentToolScopes.Resolve(definition).ShouldBe(expected);
    }

    [Fact]
    public void Inherit_clears_the_level_and_unknown_values_are_rejected()
    {
        var definition = Create();
        definition.SetAgentToolScope("ReadOnly");
        definition.SetAgentToolScope(HooshvareDefinition.AgentToolScopeInherit);
        definition.GetAgentToolScope().ShouldBeNull();

        Should.Throw<Volo.Abp.BusinessException>(() => definition.SetAgentToolScope("Admin"));
    }

    [Fact]
    public void An_unreadable_stored_level_fails_closed_to_read_only()
    {
        var definition = Create();
        definition.SetProperty(HooshvareDefinition.AgentToolScopePropertyName, "garbage");
        HooshvareAgentToolScopes.Resolve(definition).ShouldBe(AgentToolScope.ReadOnly);
        HooshvareAgentToolScopes.Resolve("7").ShouldBe(AgentToolScope.ReadOnly);
    }

    private static HooshvareDefinition Create() => (HooshvareDefinition)Activator.CreateInstance(typeof(HooshvareDefinition), nonPublic: true)!;
}
