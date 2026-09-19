using NSubstitute;

using Shouldly;

using SufiChain.SufiPlatform.SufiAI.Hooshvare;

using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

using SufiChain.SufiPlatform.Data;

using Volo.Abp;

using Xunit;



using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class PlatformHooshvareResolverTests

{

    [Fact]

    public async Task GetRuntimeIdByKeyAsync_Should_Return_Current_Scope_Hooshvare_Id()

    {

        var tenantId = Guid.NewGuid();

        var hooshvareId = Guid.NewGuid();

        const string key = PlatformHooshvareKeys.CalendarAssistant;



        var repository = Substitute.For<IHooshvareDefinitionRepository>();

        var currentTenant = Substitute.For<ICurrentTenant>();

        currentTenant.Id.Returns(tenantId);

        repository.FindByKeyAsync(key, tenantId, Arg.Any<CancellationToken>())

            .Returns(new HooshvareDefinition(

                hooshvareId,
                tenantId,
                "HelpDesk.KnowledgeBase",
                BusinessLocalizationKeys.HooshvareDisplayName(key),
                HooshvareKind.Assistant,

                "Assistant",

                Guid.NewGuid(),
                BusinessLocalizationKeys.HooshvareSystemPrompt(key),

                persistChatSession: true,

                isPublic: true,

                key: key,

                isStatic: true));



        var availabilityChecker = Substitute.For<HooshvareAvailabilityChecker>(
            Substitute.For<Volo.Abp.Features.IFeatureChecker>());
        availabilityChecker.IsAvailableAsync(Arg.Any<HooshvareDefinition>()).Returns(true);
        var resolver = new PlatformHooshvareResolver(repository, currentTenant, availabilityChecker);


        var runtimeId = await resolver.GetRuntimeIdByKeyAsync(key);



        runtimeId.ShouldBe(hooshvareId);

    }



    [Fact]

    public async Task GetRuntimeDefinitionByKeyAsync_Should_Throw_When_Hooshvare_Is_Missing()

    {

        var repository = Substitute.For<IHooshvareDefinitionRepository>();

        var currentTenant = Substitute.For<ICurrentTenant>();

        currentTenant.Id.Returns((Guid?)null);

        repository.FindByKeyAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())

            .Returns((HooshvareDefinition?)null);



        var availabilityChecker = Substitute.For<HooshvareAvailabilityChecker>(
            Substitute.For<Volo.Abp.Features.IFeatureChecker>());
        var resolver = new PlatformHooshvareResolver(repository, currentTenant, availabilityChecker);


        var exception = await Should.ThrowAsync<BusinessException>(() =>

            resolver.GetRuntimeDefinitionByKeyAsync(PlatformHooshvareKeys.ChatPublicAssistant));



        exception.Code.ShouldBe(AIHooshvareErrorCodes.HooshvareNotFound);

    }

}



public class HooshvareDefinitionStaticDeleteTests

{

    [Fact]

    public void Static_Hooshvare_Should_Be_Marked_From_Seed()

    {

        const string key = PlatformHooshvareKeys.HelpDeskKbArticleEditor;

        var definition = new HooshvareDefinition(

            Guid.NewGuid(),
            null,
            "HelpDesk.KnowledgeBase",
            BusinessLocalizationKeys.HooshvareDisplayName(key),
            HooshvareKind.Hooshvare,

            "ArticleEditor",

            Guid.NewGuid(),
            BusinessLocalizationKeys.HooshvareSystemPrompt(key),

            persistChatSession: true,

            key: key,

            isStatic: true);



        definition.IsStatic.ShouldBeTrue();

        definition.Key.ShouldBe(key);

    }

}

