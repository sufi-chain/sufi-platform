using NSubstitute;

using Shouldly;

using Volo.Abp.Features;

using Volo.Abp.MultiTenancy;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class PlatformHooshvareResolverTests

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



    private static PlatformHooshvareResolver CreateResolver(HooshvareDefinition? definition, bool featureEnabled)

    {

        var repository = Substitute.For<IHooshvareDefinitionRepository>();

        repository.FindByKeyAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())

            .Returns(definition);



        var featureChecker = Substitute.For<IFeatureChecker>();

        featureChecker.GetOrNullAsync(Arg.Any<string>()).Returns(featureEnabled ? "true" : "false");



        return new PlatformHooshvareResolver(

            repository,

            Substitute.For<ICurrentTenant>(),

            new HooshvareAvailabilityChecker(featureChecker));

    }



    private static HooshvareDefinition CreateDefinition(string key, bool enabled)

    {

        var definition = new HooshvareDefinition(

            Guid.NewGuid(),

            null,

            "HelpDesk.Ticketing",

            "Ticket Reply",

            HooshvareKind.Assistant,

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

