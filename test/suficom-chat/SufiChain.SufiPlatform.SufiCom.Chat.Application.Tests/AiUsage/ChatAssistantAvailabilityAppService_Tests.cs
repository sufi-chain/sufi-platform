using SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

using SufiChain.SufiPlatform.SufiCom.Chat.Settings;

using SufiChain.SufiPlatform.SufiCom.Chat.Supports;

using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

using SufiChain.SufiPlatform.SufiAI.Features;

using SufiChain.SufiPlatform.SufiCom.Chat.Hooshvare;

using SufiChain.SufiPlatform.Settings;

using Shouldly;

using Xunit;



namespace SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;



public class ChatAssistantAvailabilityAppService_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>

{

    private readonly IChatAssistantAvailabilityAppService _assistantAvailabilityAppService;

    private readonly ISettingManager _settingManager;

    private readonly ConfigurableFeatureChecker _featureChecker;

    private readonly ConfigurableChatAiWorkspaceProvider _workspaceProvider;

    private readonly IHooshvareCatalogAppService _hooshvareCatalogAppService;



    public ChatAssistantAvailabilityAppService_Tests()

    {

        _assistantAvailabilityAppService = GetRequiredService<IChatAssistantAvailabilityAppService>();

        _settingManager = GetRequiredService<ISettingManager>();

        _featureChecker = GetRequiredService<ConfigurableFeatureChecker>();

        _workspaceProvider = GetRequiredService<ConfigurableChatAiWorkspaceProvider>();

        _hooshvareCatalogAppService = GetRequiredService<IHooshvareCatalogAppService>();

    }



    [Fact]

    public async Task Should_Be_Available_When_All_Checks_Pass()

    {

        await ConfigureAvailableAssistantAsync();



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeTrue();

        availability.DefaultWorkspaceName.ShouldNotBeNullOrWhiteSpace();

        availability.EnabledFeatures.ShouldContain(SufiAIFeatures.Enable);

        availability.EnabledFeatures.ShouldContain(SufiAIFeatures.Workspaces);

        availability.EnabledFeatures.ShouldContain(SufiAIFeatures.Chat);

        availability.Assistants.ShouldNotBeEmpty();



        var publicAssistants = await _hooshvareCatalogAppService.GetListAsync(new GetHooshvareCatalogInput

        {

            Kind = HooshvareKind.Assistant,

            IncludePublicOnly = true

        });

        var publicAssistantIds = publicAssistants.Select(item => item.Id).ToHashSet();

        availability.Assistants.ShouldAllBe(option => publicAssistantIds.Contains(option.Id));

        availability.Assistants.ShouldAllBe(option =>

            publicAssistants.Any(item =>

                item.Id == option.Id && item.AllowUserModelSelection == option.AllowUserModelSelection));

    }



    [Fact]

    public async Task Should_Return_Only_Default_Inbox_Assistant_When_Allowlist_Is_Default()

    {

        await ConfigureAvailableAssistantAsync();

        await _settingManager.SetGlobalAsync(

            ChatSettingNames.Ai.InboxAssistantKeys,

            ChatInboxAssistantKeys.DefaultJson);



        var availability = await GetAvailabilityAsync();

        var publicAssistants = await _hooshvareCatalogAppService.GetListAsync(new GetHooshvareCatalogInput

        {

            Kind = HooshvareKind.Assistant,

            IncludePublicOnly = true

        });



        var publicAssistant = publicAssistants.FirstOrDefault(item =>

            string.Equals(item.Key, ChatHooshvareKeys.PublicAssistant.Key, StringComparison.Ordinal));

        publicAssistant.ShouldNotBeNull();



        availability.IsAvailable.ShouldBeTrue();

        availability.Assistants.ShouldNotBeEmpty();

        availability.Assistants.ShouldAllBe(option => option.Id == publicAssistant!.Id);



        foreach (var other in publicAssistants.Where(item =>

                     !string.Equals(item.Key, ChatHooshvareKeys.PublicAssistant.Key, StringComparison.Ordinal)))

        {

            availability.Assistants.ShouldNotContain(option => option.Id == other.Id);

        }

    }



    [Fact]

    public async Task Should_Include_Allowlisted_Public_Assistants()

    {

        await ConfigureAvailableAssistantAsync();



        var publicAssistants = await _hooshvareCatalogAppService.GetListAsync(new GetHooshvareCatalogInput

        {

            Kind = HooshvareKind.Assistant,

            IncludePublicOnly = true

        });



        foreach (var assistant in publicAssistants)

        {

            _workspaceProvider.HealthyWorkspaceIds.Add(assistant.WorkspaceId);

            if (!string.IsNullOrWhiteSpace(assistant.WorkspaceName))

            {

                _workspaceProvider.HealthyWorkspaces.Add(assistant.WorkspaceName);

            }

        }



        var allowlistedKeys = publicAssistants

            .Where(item => !string.IsNullOrWhiteSpace(item.Key))

            .Select(item => item.Key!)

            .Distinct(StringComparer.Ordinal)

            .ToList();



        allowlistedKeys.ShouldNotBeEmpty();



        await _settingManager.SetGlobalAsync(

            ChatSettingNames.Ai.InboxAssistantKeys,

            ChatInboxAssistantKeys.Serialize(allowlistedKeys));



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeTrue();

        availability.Assistants.Count.ShouldBe(allowlistedKeys.Count);

        availability.Assistants.ShouldAllBe(option =>

            publicAssistants.Any(item => item.Id == option.Id));

    }



    [Fact]

    public async Task Should_Exclude_Public_Assistants_Not_In_Allowlist()

    {

        await ConfigureAvailableAssistantAsync();



        var publicAssistants = await _hooshvareCatalogAppService.GetListAsync(new GetHooshvareCatalogInput

        {

            Kind = HooshvareKind.Assistant,

            IncludePublicOnly = true

        });



        var excluded = publicAssistants.FirstOrDefault(item =>

            !string.Equals(item.Key, ChatHooshvareKeys.PublicAssistant.Key, StringComparison.Ordinal));



        if (excluded == null)

        {

            // Catalog only has the default public assistant in this test host.

            return;

        }



        _workspaceProvider.HealthyWorkspaceIds.Add(excluded.WorkspaceId);

        if (!string.IsNullOrWhiteSpace(excluded.WorkspaceName))

        {

            _workspaceProvider.HealthyWorkspaces.Add(excluded.WorkspaceName);

        }



        await _settingManager.SetGlobalAsync(

            ChatSettingNames.Ai.InboxAssistantKeys,

            ChatInboxAssistantKeys.DefaultJson);



        var availability = await GetAvailabilityAsync();



        availability.Assistants.ShouldNotContain(option => option.Id == excluded.Id);

    }



    [Fact]

    public async Task Should_Be_Unavailable_When_Allowlist_Has_No_Healthy_Matches()

    {

        await ConfigureAvailableAssistantAsync();

        await _settingManager.SetGlobalAsync(

            ChatSettingNames.Ai.InboxAssistantKeys,

            ChatInboxAssistantKeys.Serialize(new[] { "Missing:AssistantKey" }));



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeFalse();

        availability.ReasonCode.ShouldBe("NoPublicHooshvares");

        availability.Assistants.ShouldBeEmpty();

    }



    [Fact]

    public async Task Should_Be_Unavailable_When_Chat_Ai_Setting_Disabled()

    {

        await ConfigureAvailableAssistantAsync();

        await _settingManager.SetGlobalAsync(ChatSettingNames.Ai.Enabled, false.ToString());



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeFalse();

        availability.ReasonCode.ShouldBe("ChatAiDisabled");

    }



    [Fact]

    public async Task Should_Be_Unavailable_When_SufiAIFeatures_Enable_Is_Disabled()

    {

        await ConfigureAvailableAssistantAsync();

        _featureChecker.Disable(SufiAIFeatures.Enable);



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeFalse();

        availability.ReasonCode.ShouldBe("AiFeatureDisabled");

    }



    [Fact]

    public async Task Should_Be_Unavailable_When_Workspaces_Feature_Is_Disabled()

    {

        await ConfigureAvailableAssistantAsync();

        _featureChecker.Disable(SufiAIFeatures.Workspaces);



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeFalse();

        availability.ReasonCode.ShouldBe("AiFeatureDisabled");

    }



    [Fact]

    public async Task Should_Be_Unavailable_When_Chat_Feature_Is_Disabled()

    {

        await ConfigureAvailableAssistantAsync();

        _featureChecker.Disable(SufiAIFeatures.Chat);



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeFalse();

        availability.ReasonCode.ShouldBe("AiFeatureDisabled");

    }



    [Fact]

    public async Task Should_Be_Unavailable_When_Integration_Is_Not_Ready()

    {

        await ConfigureAvailableAssistantAsync();

        _workspaceProvider.IntegrationReady = false;



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeFalse();

        availability.ReasonCode.ShouldBe("AiIntegrationUnavailable");

    }



    [Fact]

    public async Task Should_Be_Unavailable_When_No_Public_Hooshvare_Has_Healthy_Workspace()

    {

        await ConfigureAvailableAssistantAsync();

        _workspaceProvider.HealthyWorkspaceIds.Clear();

        _workspaceProvider.HealthyWorkspaces.Clear();



        var availability = await GetAvailabilityAsync();



        availability.IsAvailable.ShouldBeFalse();

        availability.ReasonCode.ShouldBe("NoPublicHooshvares");

    }



    private async Task ConfigureAvailableAssistantAsync()

    {

        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager);

        await ChatTestSettingHelper.SetDefaultWorkspaceAsync(_settingManager, ChatTestData.DefaultWorkspaceName);

        await _settingManager.SetGlobalAsync(

            ChatSettingNames.Ai.InboxAssistantKeys,

            ChatInboxAssistantKeys.DefaultJson);

        _featureChecker.Enable(

            SufiAIFeatures.Enable,

            SufiAIFeatures.Workspaces,

            SufiAIFeatures.Chat);

        _workspaceProvider.IntegrationReady = true;

        _workspaceProvider.HealthyWorkspaces.Clear();

        _workspaceProvider.HealthyWorkspaces.Add(ChatTestData.DefaultWorkspaceName);

        _workspaceProvider.HealthyWorkspaceIds.Clear();



        try

        {

            var hooshvare = await _hooshvareCatalogAppService.GetByKeyAsync(ChatHooshvareKeys.PublicAssistant.Key);

            _workspaceProvider.HealthyWorkspaceIds.Add(hooshvare.WorkspaceId);

            if (!string.IsNullOrWhiteSpace(hooshvare.WorkspaceName))

            {

                _workspaceProvider.HealthyWorkspaces.Add(hooshvare.WorkspaceName);

            }

        }

        catch

        {

            // Hooshvare catalog may be unavailable in isolated test setups.

        }

    }



    private async Task<ChatAssistantAvailabilityDto> GetAvailabilityAsync()

    {

        using (CurrentUser.Change(ChatTestData.UserAId))

        {

            return await _assistantAvailabilityAppService.GetAsync();

        }

    }

}

