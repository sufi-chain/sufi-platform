using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using SufiChain.SufiPlatform.Settings;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Composer;

public class ChatComposerCapabilitiesAppService_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatComposerCapabilitiesAppService _capabilitiesAppService;

    public ChatComposerCapabilitiesAppService_Tests()
    {
        _capabilitiesAppService = GetRequiredService<IChatComposerCapabilitiesAppService>();
    }

    [Fact]
    public async Task Should_Disable_Rich_Composer_For_Anonymous_User()
    {
        using (CurrentUser.Change(null))
        {
            var capabilities = await _capabilitiesAppService.GetAsync();

            capabilities.CanUseRichComposer.ShouldBeFalse();
            capabilities.CanAttachFiles.ShouldBeFalse();
            capabilities.CanShareLocation.ShouldBeFalse();
            capabilities.CanRecordVoice.ShouldBeFalse();
            capabilities.CanUseOperatorCopilot.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Should_Enable_Rich_Composer_For_Authenticated_User_When_Settings_Allow()
    {
        var settingManager = GetRequiredService<ISettingManager>();
        await ChatTestSettingHelper.SetAnonymousUsagePolicyAsync(settingManager);
        await settingManager.SetGlobalAsync(ChatSettingNames.General.EnableFileAttachments, true.ToString());
        await settingManager.SetGlobalAsync(ChatSettingNames.Attachments.EnableLocationSharing, true.ToString());
        await settingManager.SetGlobalAsync(ChatSettingNames.Attachments.EnableVoiceMessages, true.ToString());

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var capabilities = await _capabilitiesAppService.GetAsync();

            capabilities.CanUseRichComposer.ShouldBeTrue();
            capabilities.CanAttachFiles.ShouldBeTrue();
            capabilities.CanShareLocation.ShouldBeTrue();
            capabilities.CanRecordVoice.ShouldBeTrue();
        }
    }
}
