using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class McpServerAdminRulesTests
{
    [Fact]
    public void Display_name_is_unique_per_tenant_without_regard_to_case()
    {
        var existing = new[] { (Id: Guid.NewGuid(), Name: "Baneh") };

        McpServerAdminRules.IsDuplicateName(existing, currentId: null, "baneh").ShouldBeTrue();
        McpServerAdminRules.IsDuplicateName(existing, existing[0].Id, "Baneh").ShouldBeFalse();
        McpServerAdminRules.IsDuplicateName(existing, currentId: null, "Other").ShouldBeFalse();
    }

    [Fact]
    public void Blank_or_relative_endpoint_fails_dto_validation_for_http()
    {
        var blank = new CreateMCPServerDto
        {
            Key = "baneh",
            Name = "Baneh",
            TransportType = "HTTP",
            Endpoint = "   "
        };
        var relative = new UpdateMCPServerDto
        {
            Name = "Baneh",
            TransportType = "SSE",
            Endpoint = "/mcp"
        };
        var stdio = new CreateMCPServerDto
        {
            Key = "local",
            Name = "Local",
            TransportType = "STDIO"
        };

        Errors(blank).ShouldContain(result => result.MemberNames.Contains(nameof(CreateMCPServerDto.Endpoint)));
        Errors(relative).ShouldContain(result => result.MemberNames.Contains(nameof(UpdateMCPServerDto.Endpoint)));
        Errors(stdio).ShouldBeEmpty();
        McpServerAdminRules.IsAbsoluteHttpUrl("https://mcp.example.com/mcp").ShouldBeTrue();
    }

    private static List<ValidationResult> Errors(IValidatableObject dto) =>
        dto.Validate(new ValidationContext(dto)).ToList();
}
