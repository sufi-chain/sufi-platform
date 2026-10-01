using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiCRM.Contacts.Data;
using SufiChain.SufiPlatform.SufiCRM.Contacts.Hooshvare;
using Volo.Abp.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCRM.Contacts;

public class CrmOnboardingHooshvareSeedContractTests
{
    [Fact]
    public async Task Contributor_Should_Require_Invitation_Session_And_Contact()
    {
        var definitions = new List<PlatformHooshvareSeedDefinition>();
        var seeder = Substitute.For<IPlatformHooshvareDefinitionSeeder>();
        seeder.SeedAsync(Arg.Any<PlatformHooshvareSeedDefinition>(), Arg.Any<DataSeedContext>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                definitions.Add(call.Arg<PlatformHooshvareSeedDefinition>());
                return Task.CompletedTask;
            });
        var localization = Substitute.For<ILocalizationTextSeeder>();
        localization.UpsertAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                Arg.Any<Guid?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var contributor = new CrmOnboardingHooshvareDataSeedContributor(seeder, localization);

        await contributor.SeedAsync(new DataSeedContext(null));

        var definition = definitions.ShouldHaveSingleItem();
        definition.Key.ShouldBe(CrmOnboardingHooshvareKeys.Key);
        definition.RequiredContextKeys.ShouldBe(["invitationToken", "sessionId", "contactName"]);
        definition.RequiredContextKeys.ShouldNotContain("firstName");
        definition.RuntimeOptions.UseMcpTools.ShouldBeTrue();
        definition.IsPublic.ShouldBeTrue();
    }
}
