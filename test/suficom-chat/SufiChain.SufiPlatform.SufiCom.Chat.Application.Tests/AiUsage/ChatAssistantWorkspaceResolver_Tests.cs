using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using SufiChain.SufiPlatform.SufiCom.Chat.Copilots;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using SufiChain.SufiPlatform.SufiCom.Chat.Supports;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

public class ChatAssistantWorkspaceResolver_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatAssistantWorkspaceResolver _resolver;
    private readonly ConfigurableChatAiWorkspaceProvider _workspaceProvider;

    public ChatAssistantWorkspaceResolver_Tests()
    {
        _resolver = GetRequiredService<IChatAssistantWorkspaceResolver>();
        _workspaceProvider = GetRequiredService<ConfigurableChatAiWorkspaceProvider>();
    }

    [Fact]
    public async Task Should_Use_Explicit_Workspace_Name()
    {
        _workspaceProvider.IntegrationReady = true;
        _workspaceProvider.HealthyWorkspaces.Add("sales");

        var workspaceName = await _resolver.ResolveWorkspaceNameAsync(new ChatAssistantWorkspaceResolveContext
        {
            ExplicitWorkspaceName = "sales"
        });

        workspaceName.ShouldBe("sales");
    }

    [Fact]
    public async Task Should_Read_Workspace_From_Assistant_Context()
    {
        await ChatTestSettingHelper.SetDefaultWorkspaceAsync(GetRequiredService<SufiChain.SufiPlatform.Settings.ISettingManager>(), ChatTestData.DefaultWorkspaceName);

        var workspaceName = await _resolver.ResolveWorkspaceNameAsync(new ChatAssistantWorkspaceResolveContext
        {
            AssistantWorkspaceName = "support"
        });

        workspaceName.ShouldBe("support");
    }

    [Fact]
    public async Task Should_Resolve_Workspace_From_Assistant_Id()
    {
        var catalog = GetRequiredService<ICopilotCatalogAppService>();
        var assistantId = (await catalog.GetByKeyAsync(ChatCopilotKeys.PublicAssistant.Key)).Id;

        var workspaceName = await _resolver.ResolveWorkspaceNameAsync(new ChatAssistantWorkspaceResolveContext
        {
            AssistantId = assistantId,
            AssistantWorkspaceName = "sales"
        });

        workspaceName.ShouldBe("sales");
    }
}
