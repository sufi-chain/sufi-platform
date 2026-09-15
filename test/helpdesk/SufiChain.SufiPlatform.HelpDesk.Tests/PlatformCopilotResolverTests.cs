using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Copilots;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using SufiChain.SufiPlatform.Data;
using Volo.Abp;
using Xunit;

using Volo.Abp.MultiTenancy;
namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class PlatformCopilotResolverTests
{
    [Fact]
    public async Task GetRuntimeIdByKeyAsync_Should_Return_Current_Scope_Copilot_Id()
    {
        var tenantId = Guid.NewGuid();
        var copilotId = Guid.NewGuid();
        const string key = PlatformCopilotKeys.CalendarAssistant;

        var repository = Substitute.For<ICopilotDefinitionRepository>();
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        repository.FindByKeyAsync(key, tenantId, Arg.Any<CancellationToken>())
            .Returns(new CopilotDefinition(
                copilotId,
                tenantId,
                "HelpDesk.KnowledgeBase",
                BusinessLocalizationKeys.CopilotDisplayName(key),
                CopilotKind.Assistant,
                "Assistant",
                Guid.NewGuid(),
                BusinessLocalizationKeys.CopilotSystemPrompt(key),
                persistChatSession: true,
                isPublic: true,
                key: key,
                isStatic: true));

        var availabilityChecker = Substitute.For<CopilotAvailabilityChecker>(
            Substitute.For<Volo.Abp.Features.IFeatureChecker>());
        availabilityChecker.IsAvailableAsync(Arg.Any<CopilotDefinition>()).Returns(true);
        var resolver = new PlatformCopilotResolver(repository, currentTenant, availabilityChecker);

        var runtimeId = await resolver.GetRuntimeIdByKeyAsync(key);

        runtimeId.ShouldBe(copilotId);
    }

    [Fact]
    public async Task GetRuntimeDefinitionByKeyAsync_Should_Throw_When_Copilot_Is_Missing()
    {
        var repository = Substitute.For<ICopilotDefinitionRepository>();
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns((Guid?)null);
        repository.FindByKeyAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns((CopilotDefinition?)null);

        var availabilityChecker = Substitute.For<CopilotAvailabilityChecker>(
            Substitute.For<Volo.Abp.Features.IFeatureChecker>());
        var resolver = new PlatformCopilotResolver(repository, currentTenant, availabilityChecker);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            resolver.GetRuntimeDefinitionByKeyAsync(PlatformCopilotKeys.ChatPublicAssistant));

        exception.Code.ShouldBe(AICopilotsErrorCodes.CopilotNotFound);
    }
}

public class CopilotDefinitionStaticDeleteTests
{
    [Fact]
    public void Static_Copilot_Should_Be_Marked_From_Seed()
    {
        const string key = PlatformCopilotKeys.HelpDeskKbArticleEditor;
        var definition = new CopilotDefinition(
            Guid.NewGuid(),
            null,
            "HelpDesk.KnowledgeBase",
            BusinessLocalizationKeys.CopilotDisplayName(key),
            CopilotKind.Copilot,
            "ArticleEditor",
            Guid.NewGuid(),
            BusinessLocalizationKeys.CopilotSystemPrompt(key),
            persistChatSession: true,
            key: key,
            isStatic: true);

        definition.IsStatic.ShouldBeTrue();
        definition.Key.ShouldBe(key);
    }
}
