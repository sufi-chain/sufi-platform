using NSubstitute;
using Shouldly;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class PlatformCopilotResolverTests
{
    [Fact]
    public async Task TryGetRuntimeIdByKeyAsync_Should_Return_Empty_When_Definition_Is_Missing()
    {
        var resolver = CreateResolver(definition: null, featureEnabled: true);

        var id = await resolver.TryGetRuntimeIdByKeyAsync("SufiHelpDesk:AgentReply");

        id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public async Task TryGetRuntimeIdByKeyAsync_Should_Return_Empty_When_Definition_Is_Disabled()
    {
        var definition = CreateDefinition("SufiHelpDesk:AgentReply", enabled: false);
        var resolver = CreateResolver(definition, featureEnabled: true);

        var id = await resolver.TryGetRuntimeIdByKeyAsync(definition.Key!);

        id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public async Task TryGetRuntimeIdByKeyAsync_Should_Return_Id_When_Available()
    {
        var definition = CreateDefinition("SufiHelpDesk:AgentReply", enabled: true);
        var resolver = CreateResolver(definition, featureEnabled: true);

        var id = await resolver.TryGetRuntimeIdByKeyAsync(definition.Key!);

        id.ShouldBe(definition.Id);
    }

    private static PlatformCopilotResolver CreateResolver(CopilotDefinition? definition, bool featureEnabled)
    {
        var repository = Substitute.For<ICopilotDefinitionRepository>();
        repository.FindByKeyAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(definition);

        var featureChecker = Substitute.For<IFeatureChecker>();
        featureChecker.GetOrNullAsync(Arg.Any<string>()).Returns(featureEnabled ? "true" : "false");

        return new PlatformCopilotResolver(
            repository,
            Substitute.For<ICurrentTenant>(),
            new CopilotAvailabilityChecker(featureChecker));
    }

    private static CopilotDefinition CreateDefinition(string key, bool enabled)
    {
        var definition = new CopilotDefinition(
            Guid.NewGuid(),
            null,
            "HelpDesk.Ticketing",
            "Ticket Reply",
            CopilotKind.Assistant,
            "AgentReply",
            Guid.NewGuid(),
            "prompt",
            persistChatSession: false,
            key: key,
            defaultEnabled: enabled);

        if (!enabled)
        {
            definition.Disable();
        }

        return definition;
    }
}
