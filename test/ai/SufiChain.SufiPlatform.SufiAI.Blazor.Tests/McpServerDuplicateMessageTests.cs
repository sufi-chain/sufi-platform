using Bunit;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Blazor.Components;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Tests;

public class McpServerDuplicateMessageTests
{
    [Fact]
    public void Persian_key_duplicate_text_is_present()
    {
        var json = Read("fa.json");
        json.ShouldContain("\"AI:MCPServerKeyAlreadyExists\"");
        json.ShouldContain("سرور MCP با این کلید از قبل وجود دارد.");
        Read("en.json").ShouldContain("An MCP server with this key already exists.");
        Read("ar.json").ShouldContain("يوجد بالفعل خادم MCP بهذا المفتاح.");
        Read("es.json").ShouldContain("Ya existe un servidor MCP con esta clave.");
    }

    [Fact]
    public void Key_and_name_codes_map_to_their_fields()
    {
        McpServerDuplicateFields.Field(AIErrorCodes.MCPServerKeyAlreadyExists).ShouldBe(McpServerDuplicateFields.Key);
        McpServerDuplicateFields.Field(AIErrorCodes.MCPServerNameAlreadyExists).ShouldBe(McpServerDuplicateFields.Name);
        McpServerDuplicateFields.Field(AIErrorCodes.MCPEndpointInvalid).ShouldBeNull();
    }

    private static string Read(string file)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(
                dir.FullName,
                "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Domain.Shared/Localization/AI",
                file);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(file);
    }
}

public class McpServerDuplicateNoticeTests : BunitContext
{
    [Theory]
    [InlineData("Key", "سرور MCP با این کلید از قبل وجود دارد.")]
    [InlineData("Name", "سرور MCP با این نام از قبل وجود دارد.")]
    public void Notice_shows_the_localized_field_message(string field, string text)
    {
        var cut = Render<McpServerDuplicateNotice>(parameters => parameters
            .Add(component => component.Field, field)
            .Add(component => component.Text, text));

        cut.Markup.ShouldContain(text);
        cut.Markup.ShouldContain($"data-field=\"{field}\"");
    }
}
