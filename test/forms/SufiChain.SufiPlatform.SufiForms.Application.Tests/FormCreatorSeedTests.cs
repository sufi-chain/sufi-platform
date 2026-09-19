using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiForms.Data;
using Volo.Abp.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormCreatorSeedTests
{
    [Fact]
    public async Task Seed_is_private_tenant_aware_and_allows_only_definition_tools()
    {
        var definitions = new List<PlatformHooshvareSeedDefinition>();
        var seeder = Substitute.For<IPlatformHooshvareDefinitionSeeder>();
        var tenantId = Guid.NewGuid();
        seeder.SeedAsync(Arg.Any<PlatformHooshvareSeedDefinition>(), Arg.Any<DataSeedContext>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                call.Arg<DataSeedContext>().TenantId.ShouldBe(tenantId);
                definitions.Add(call.Arg<PlatformHooshvareSeedDefinition>());
                return Task.CompletedTask;
            });
        var contributor = new FormCreatorHooshvareDataSeedContributor(seeder, Substitute.For<ILocalizationTextSeeder>());
        await contributor.SeedAsync(new DataSeedContext(tenantId));
        var definition = definitions.ShouldHaveSingleItem();
        definition.Key.ShouldBe(PlatformHooshvareKeys.SufiFormsFormCreator);
        definition.IsPublic.ShouldBeFalse();
        definition.PersistChatSession.ShouldBeFalse();
        definition.RuntimeOptions.AllowedMcpToolNames.ShouldBe(new List<string>
        {
            "forms.get_designer_catalog", "forms.get_definition", "forms.create_definition", "forms.update_definition"
        });
        definition.RequiredContextKeys.ShouldBe(new List<string> { "operation", "definitionId", "phase", "proposal" });
        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            FormCreatorHooshvareSeedTexts.Texts.DisplayName[culture].ShouldNotBeNullOrWhiteSpace();
            FormCreatorHooshvareSeedTexts.Texts.SystemPrompt[culture].ShouldContain("RendererLimitations");
            foreach (var name in definition.RuntimeOptions.AllowedMcpToolNames)
            {
                FormsMcpToolSeedTexts.Get(name).DisplayNames[culture].ShouldNotBeNullOrWhiteSpace();
                FormsMcpToolSeedTexts.Get(name).Descriptions[culture].ShouldNotBeNullOrWhiteSpace();
            }
        }
    }
}
