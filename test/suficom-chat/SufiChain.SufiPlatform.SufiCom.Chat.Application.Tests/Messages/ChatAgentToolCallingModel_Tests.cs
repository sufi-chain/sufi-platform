using System.Text.Json;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

public class ChatAgentToolCallingModel_Tests
{
    [Fact]
    public void AgentKind_NoToolCallingModel_SendsLocalizedErrorAndNoPlainReply()
    {
        var en = Read("en.json");
        var fa = Read("fa.json");
        var ar = Read("ar.json");
        var es = Read("es.json");

        en.ShouldContain("needs a model that supports tool calling");
        fa.ShouldContain("عامل گفت\u200cوگو");
        fa.ShouldContain("فراخوانی ابزار");
        ar.ShouldContain("استدعاء الأدوات");
        es.ShouldContain("llamadas a herramientas");

        var handler = File.ReadAllText(FindRepoFile(
            "pro-modules", "suficom", "chat", "src",
            "SufiChain.SufiPlatform.SufiCom.Chat.Application", "Messages",
            "ChatAssistantMessageHandler.cs"));
        var emptyBody = handler.IndexOf("throw new BusinessException(ChatErrorCodes.AiUnavailable)", StringComparison.Ordinal);
        var secondCall = handler.IndexOf("var agentKind = await IsAgentKindAsync", StringComparison.Ordinal);
        emptyBody.ShouldBeGreaterThan(0);
        secondCall.ShouldBeGreaterThan(emptyBody);
        handler.ShouldContain("ChatErrorCodes.AgentNeedsToolCallingModel");
        handler.ShouldContain("turnTimeout.Token");
    }

    private static string Read(string fileName)
    {
        var path = FindRepoFile(
            "pro-modules", "suficom", "chat", "src",
            "SufiChain.SufiPlatform.SufiCom.Chat.Domain.Shared", "Localization", "Chat", fileName);
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.GetProperty("texts").GetProperty("Chat:AgentNeedsToolCallingModel").GetString()!;
    }

    private static string FindRepoFile(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(new[] { dir.FullName }.Concat(parts).ToArray());
            if (File.Exists(path))
            {
                return path;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(string.Join('/', parts));
    }
}
