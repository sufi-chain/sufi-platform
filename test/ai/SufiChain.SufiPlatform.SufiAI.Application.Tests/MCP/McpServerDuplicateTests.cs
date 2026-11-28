using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.MCP;

public class McpServerDuplicateTests : SufiAITestBase<McpEnvironmentAuditTestModule>
{
    [Fact]
    public async Task Duplicate_key_uses_the_key_code()
    {
        var service = GetRequiredService<IMCPServerAppService>();
        var key = "dup-key-" + Guid.NewGuid().ToString("N");
        await service.CreateAsync(Stdio(key, "First " + key));

        var error = await Should.ThrowAsync<BusinessException>(() => service.CreateAsync(Stdio(key, "Second " + key)));

        error.Code.ShouldBe(AIErrorCodes.MCPServerKeyAlreadyExists);
    }

    [Fact]
    public async Task Duplicate_name_with_a_new_key_uses_the_name_code()
    {
        var service = GetRequiredService<IMCPServerAppService>();
        var name = "Same name " + Guid.NewGuid().ToString("N");
        await service.CreateAsync(Stdio("name-a-" + Guid.NewGuid().ToString("N"), name));

        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.CreateAsync(Stdio("name-b-" + Guid.NewGuid().ToString("N"), name)));

        error.Code.ShouldBe(AIErrorCodes.MCPServerNameAlreadyExists);
    }

    [Fact]
    public async Task Update_to_a_duplicate_name_is_rejected()
    {
        var service = GetRequiredService<IMCPServerAppService>();
        var first = await service.CreateAsync(Stdio("upd-a-" + Guid.NewGuid().ToString("N"), "Kept " + Guid.NewGuid().ToString("N")));
        var second = await service.CreateAsync(Stdio("upd-b-" + Guid.NewGuid().ToString("N"), "Other " + Guid.NewGuid().ToString("N")));

        var error = await Should.ThrowAsync<BusinessException>(() => service.UpdateAsync(second.Id, new UpdateMCPServerDto
        {
            Name = first.Name,
            TransportType = "STDIO",
            Command = "python3"
        }));

        error.Code.ShouldBe(AIErrorCodes.MCPServerNameAlreadyExists);
    }

    private static CreateMCPServerDto Stdio(string key, string name) => new()
    {
        Key = key,
        Name = name,
        TransportType = "STDIO",
        Command = "python3",
        BindingMode = "TenantLevel"
    };
}
