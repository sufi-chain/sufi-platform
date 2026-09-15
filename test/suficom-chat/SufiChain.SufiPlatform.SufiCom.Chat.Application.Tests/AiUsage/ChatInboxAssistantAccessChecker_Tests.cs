using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using SufiChain.SufiPlatform.SufiCom.Chat.Copilots;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using Volo.Abp.Settings;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

public class ChatInboxAssistantAccessChecker_Tests
{
    private static readonly Guid AssistantId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid RoleId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    [Fact]
    public async Task Empty_Roles_Should_Allow_Authenticated_User()
    {
        var checker = CreateChecker(Array.Empty<Guid>(), Array.Empty<string>());

        (await checker.CanAccessAsync(AssistantId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Any_Matching_Role_Should_Allow_User()
    {
        var checker = CreateChecker(new[] { RoleId }, new[] { "Support" });

        (await checker.CanAccessAsync(AssistantId)).ShouldBeTrue();
    }

    [Fact]
    public async Task Missing_Matching_Role_Should_Deny_User()
    {
        var checker = CreateChecker(new[] { RoleId }, new[] { "Sales" });

        (await checker.CanAccessAsync(AssistantId)).ShouldBeFalse();
    }

    private static ChatInboxAssistantAccessChecker CreateChecker(
        IReadOnlyCollection<Guid> roleIds,
        IReadOnlyCollection<string> currentUserRoles)
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider
            .GetOrNullAsync(ChatSettingNames.Ai.InboxAssistantKeys)
            .Returns(ChatInboxAssistantKeys.DefaultJson);
        settingProvider
            .GetOrNullAsync(ChatSettingNames.Ai.InboxAssistantAccessRules)
            .Returns(ChatInboxAssistantAccessRules.Serialize(new[]
            {
                new ChatInboxAssistantAccessRule
                {
                    Key = ChatCopilotKeys.PublicAssistant.Key,
                    RoleIds = roleIds.ToList()
                }
            }));

        var copilotCatalog = Substitute.For<ICopilotCatalogAppService>();
        copilotCatalog.GetAsync(AssistantId).Returns(new CopilotCatalogItemDto
        {
            Id = AssistantId,
            Key = ChatCopilotKeys.PublicAssistant.Key,
            DisplayName = "Everyday Assistant",
            Kind = CopilotKind.Assistant,
            IsPublic = true
        });

        var roleRepository = Substitute.For<IIdentityRoleRepository>();
        roleRepository
            .GetListAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(roleIds.Count == 0
                ? new List<IdentityRole>()
                : new List<IdentityRole>
                {
                    new(RoleId, "Support")
                });

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.Roles.Returns(currentUserRoles.ToArray());

        return new ChatInboxAssistantAccessChecker(
            settingProvider,
            copilotCatalog,
            roleRepository,
            currentUser);
    }
}
