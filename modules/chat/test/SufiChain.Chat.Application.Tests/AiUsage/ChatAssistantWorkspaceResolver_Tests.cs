using Shouldly;
using SufiChain.Chat.Settings;
using Xunit;

namespace SufiChain.Chat.AiUsage;

public class ChatAssistantWorkspaceResolver_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
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
    public async Task Should_Read_Workspace_From_Session_Metadata()
    {
        await ChatTestSettingHelper.SetDefaultWorkspaceAsync(GetRequiredService<SufiChain.SufiAbp.SettingManagement.ISettingManager>(), ChatTestData.DefaultWorkspaceName);

        var workspaceName = await _resolver.ResolveWorkspaceNameAsync(new ChatAssistantWorkspaceResolveContext
        {
            SessionMetadataJson = ChatAssistantMetadata.BuildJson("support")
        });

        workspaceName.ShouldBe("support");
    }

    [Fact]
    public async Task Should_Resolve_Workspace_From_Assistant_Key()
    {
        var settingManager = GetRequiredService<SufiChain.SufiAbp.SettingManagement.ISettingManager>();
        await settingManager.SetAsync(
            ChatSettingNames.Ai.AssistantMappings,
            ChatAssistantMappings.Serialize(new[]
            {
                new ChatAssistantMappingItem
                {
                    Key = "sales",
                    DisplayName = "Sales",
                    WorkspaceName = "sales",
                    IsEnabled = true
                }
            }),
            tenantId: null);

        _workspaceProvider.IntegrationReady = true;
        _workspaceProvider.HealthyWorkspaces.Add("sales");

        var workspaceName = await _resolver.ResolveWorkspaceNameAsync(new ChatAssistantWorkspaceResolveContext
        {
            AssistantKey = "sales"
        });

        workspaceName.ShouldBe("sales");
    }
}
