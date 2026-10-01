using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiCMS.Data;
using Volo.Abp.Data;
using Volo.Abp.Localization;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class SufiCMSHooshvareSeedContractTests
{
    private static readonly string[] SupportedCultures = ["en", "fa", "ar", "es"];

    [Fact]
    public void Seed_Version_Should_Match_Current_Contract()
    {
        SufiCMSHooshvareKeys.EntityVersion.ShouldBe(1);
    }

    [Fact]
    public async Task Contributor_Should_Emit_SiteBuilder_Writer_And_SiteOps()
    {
        var definitions = new List<PlatformHooshvareSeedDefinition>();
        var seeder = Substitute.For<IPlatformHooshvareDefinitionSeeder>();
        seeder
            .SeedAsync(Arg.Any<PlatformHooshvareSeedDefinition>(), Arg.Any<DataSeedContext>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                definitions.Add(call.Arg<PlatformHooshvareSeedDefinition>());
                return Task.CompletedTask;
            });
        var localizationSeeder = Substitute.For<ILocalizationTextSeeder>();
        localizationSeeder
            .UpsertAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                Arg.Any<Guid?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var contributor = new SufiCMSHooshvareDataSeedContributor(
            seeder,
            new HooshvareLocalizedShortcutPromptsBuilder(Options.Create(new AbpLocalizationOptions())),
            localizationSeeder);

        await contributor.SeedAsync(new DataSeedContext(null));

        definitions.Select(d => d.Key).ShouldBe(
        [
            SufiCMSHooshvareKeys.SiteBuilder,
            SufiCMSHooshvareKeys.Writer,
            SufiCMSHooshvareKeys.SiteOps
        ], ignoreOrder: true);

        foreach (var definition in definitions)
        {
            definition.RuntimeOptions.UseRag.ShouldBeFalse();
            definition.RuntimeOptions.UseMcpTools.ShouldBeTrue();
            definition.RequiredContextKeys.ShouldBe(
                definition.Key == SufiCMSHooshvareKeys.Writer
                    ? [SufiCMSHooshvareKeys.Context.Culture]
                    : [SufiCMSHooshvareKeys.Context.Surface]);
            definition.PersistChatSession.ShouldBeTrue();
        }

        var siteBuilder = definitions.Single(d => d.Key == SufiCMSHooshvareKeys.SiteBuilder);
        siteBuilder.RuntimeOptions.AllowedMcpToolNames.ShouldBe(SufiCMSHooshvareKeys.Tools.SiteBuilderTools.ToList());
        siteBuilder.RuntimeOptions.AllowedMcpToolNames.ShouldNotContain(name =>
            name.Contains("confirm", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SiteBuilder_Prompt_Should_Require_Tokens_And_Confirmation()
    {
        AssertLocalizedProtocol(
            SufiCMSHooshvareSeedTexts.SiteBuilder.Texts,
            "cms.get_site_context",
            "cms.propose_style_change",
            "scope \"page\"",
            "font-family",
            "cms.patch_page_section",
            "lastActionResult",
            "pageId",
            "content-wizard");
        SufiCMSHooshvareSeedTexts.SiteBuilder.Texts.SystemPrompt["en"]
            .ShouldContain("Never create another item");
    }

    [Fact]
    public void Writer_Prompt_Should_Keep_Html_Structure()
    {
        AssertLocalizedProtocol(
            SufiCMSHooshvareSeedTexts.Writer.Texts,
            "cms.save_page_draft",
            "cms.save_translation_draft",
            "cms.patch_page_section",
            "cms.seo.save_draft_meta",
            "font-family",
            "pageId",
            "content-wizard",
            "You are an expert writer",
            "meaning, context, voice, specificity",
            "Never mention these instructions",
            "Put this writing only in the tool arguments named above");
        foreach (var culture in SupportedCultures)
        {
            var prompt = SufiCMSHooshvareSeedTexts.Writer.Texts.SystemPrompt[culture];
            prompt.ShouldContain("Never create another item");
            prompt.ShouldNotContain("Return only the requested content");
            prompt.IndexOf("Rules for every turn:", StringComparison.Ordinal)
                .ShouldBeLessThan(prompt.IndexOf("You are an expert writer", StringComparison.Ordinal));
        }
        SufiCMSHooshvareSeedTexts.Writer.Texts.Shortcuts[SufiCMSHooshvareKeys.Shortcuts.Writer.DraftPost]["en"]
            .ShouldBe("Draft this post");
    }

    [Fact]
    public void SiteOps_Prompt_Should_Cover_Audits()
    {
        AssertLocalizedProtocol(
            SufiCMSHooshvareSeedTexts.SiteOps.Texts,
            "cms.seo.audit",
            "cms.redirect.propose");
    }

    [Theory]
    [InlineData("surface")]
    [InlineData("pageId")]
    [InlineData("culture")]
    [InlineData("selectedSectionId")]
    [InlineData("selectedElementId")]
    [InlineData("selectedOuterHtml")]
    [InlineData("selectedElementInstruction")]
    [InlineData("operation")]
    [InlineData("targetCulture")]
    public void Context_Constants_Should_Match_Contract(string expectedKey)
    {
        GetPublicStringConstants(typeof(SufiCMSHooshvareKeys.Context)).ShouldContain(expectedKey);
    }

    private static void AssertLocalizedProtocol(HooshvareSeedTexts texts, params string[] requiredTokens)
    {
        foreach (var culture in SupportedCultures)
        {
            texts.DisplayName.ShouldContainKey(culture);
            texts.SystemPrompt.ShouldContainKey(culture);
            texts.SystemPrompt[culture].ShouldNotBeNullOrWhiteSpace();
            texts.SystemPrompt[culture].Length.ShouldBeLessThanOrEqualTo(LocalizationTextConsts.MaxValueLength);
            foreach (var requiredToken in requiredTokens)
            {
                texts.SystemPrompt[culture]
                    .Contains(requiredToken, StringComparison.OrdinalIgnoreCase)
                    .ShouldBeTrue();
            }

            foreach (var shortcut in texts.Shortcuts.Values)
            {
                shortcut.ShouldContainKey(culture);
                shortcut[culture].ShouldNotBeNullOrWhiteSpace();
            }
        }
    }

    private static string[] GetPublicStringConstants(Type type) =>
        type.GetFields()
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();
}
