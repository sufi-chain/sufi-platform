using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using SufiChain.SufiPlatform.SufiCMS.Data;
using Volo.Abp.Data;
using Volo.Abp.Localization;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class SufiCMSCopilotSeedContractTests
{
    private static readonly string[] SupportedCultures = ["en", "fa", "ar", "es"];

    [Fact]
    public void Seed_Version_Should_Reflect_NonPersistent_Context_Hardening()
    {
        SufiCMSCopilotKeys.EntityVersion.ShouldBe(6);
    }

    [Fact]
    public async Task Contributor_Should_Emit_NonPersistent_Definitions_With_Canonical_Context()
    {
        var definitions = new List<PlatformCopilotSeedDefinition>();
        var seeder = Substitute.For<IPlatformCopilotDefinitionSeeder>();
        seeder
            .SeedAsync(Arg.Any<PlatformCopilotSeedDefinition>(), Arg.Any<DataSeedContext>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                definitions.Add(call.Arg<PlatformCopilotSeedDefinition>());
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
        var shortcutBuilder = new CopilotLocalizedShortcutPromptsBuilder(
            Options.Create(new AbpLocalizationOptions()));
        var contributor = new SufiCMSCopilotDataSeedContributor(
            seeder,
            shortcutBuilder,
            localizationSeeder);

        await contributor.SeedAsync(new DataSeedContext(null));

        definitions.Count.ShouldBe(3);
        definitions.All(definition => !definition.PersistChatSession).ShouldBeTrue();

        var pageDesigner = definitions.Single(definition => definition.Key == SufiCMSCopilotKeys.PageDesigner);
        pageDesigner.RequiredContextKeys.ShouldBe(new List<string>
        {
            "editorTree",
            "authoringMode",
            "enabledElementKeys",
            "frontendGuidanceProjectSlug",
            "catalogRefresh"
        });
        pageDesigner.RuntimeOptions.UseMcpTools.ShouldBeTrue();
        pageDesigner.RuntimeOptions.AllowedMcpToolNames.ShouldBe(new List<string>
        {
            SufiCMSCopilotKeys.Tools.DesignerCatalog,
            SufiCMSCopilotKeys.Tools.ProjectGuidance
        });

        var widgetDesigner = definitions.Single(definition => definition.Key == SufiCMSCopilotKeys.WidgetDesigner);
        widgetDesigner.RequiredContextKeys.ShouldContain("allowedValues");
        widgetDesigner.RuntimeOptions.UseMcpTools.ShouldBeFalse();

        var contentAssistant = definitions.Single(definition => definition.Key == SufiCMSCopilotKeys.ContentAssistant);
        contentAssistant.RequiredContextKeys.ShouldContain("outputFields");
        contentAssistant.RuntimeOptions.UseMcpTools.ShouldBeFalse();
    }

    [Fact]
    public void Page_Designer_Should_Define_Equivalent_Localized_Protocols()
    {
        AssertLocalizedProtocol(
            SufiCMSCopilotSeedTexts.PageDesigner.Texts,
            "cms.get_designer_catalog",
            "helpdesk.kb.search_project_guidance",
            "editorTree",
            "frontendGuidanceProjectSlug",
            "JSON",
            "RenderNodeDto");
    }

    [Fact]
    public void Widget_Designer_Should_Use_Schema_Backed_Shortcuts_And_Protocols()
    {
        var texts = SufiCMSCopilotSeedTexts.WidgetDesigner.Texts;

        texts.Shortcuts.Keys.ShouldContain(SufiCMSCopilotKeys.Shortcuts.WidgetDesigner.ValidateOptions);
        texts.Shortcuts.Keys.ShouldNotContain("BlockConfig");
        AssertLocalizedProtocol(
            texts,
            "widgetKey",
            "currentOptions",
            "optionSchema",
            "allowedValues",
            "JSON",
            "unsupported_widget_option");
    }

    [Fact]
    public void Content_Assistant_Should_Define_Operation_Specific_Output_Protocol()
    {
        AssertLocalizedProtocol(
            SufiCMSCopilotSeedTexts.ContentAssistant.Texts,
            "operation",
            "sourceText",
            "targetCulture",
            "outputFields",
            "translate",
            "rewrite",
            "summarize",
            "seo",
            "JSON");
    }

    [Theory]
    [InlineData("editorTree")]
    [InlineData("authoringMode")]
    [InlineData("enabledElementKeys")]
    [InlineData("frontendGuidanceProjectSlug")]
    [InlineData("catalogRefresh")]
    public void Page_Designer_Context_Constants_Should_Match_Contract(string expectedKey)
    {
        GetPublicStringConstants(typeof(SufiCMSCopilotKeys.Context.PageDesigner))
            .ShouldContain(expectedKey);
    }

    [Theory]
    [InlineData("widgetKey")]
    [InlineData("widgetType")]
    [InlineData("currentOptions")]
    [InlineData("optionSchema")]
    [InlineData("allowedValues")]
    public void Widget_Designer_Context_Constants_Should_Match_Contract(string expectedKey)
    {
        GetPublicStringConstants(typeof(SufiCMSCopilotKeys.Context.WidgetDesigner))
            .ShouldContain(expectedKey);
    }

    [Theory]
    [InlineData("operation")]
    [InlineData("sourceText")]
    [InlineData("sourceCulture")]
    [InlineData("targetCulture")]
    [InlineData("outputFields")]
    public void Content_Assistant_Context_Constants_Should_Match_Contract(string expectedKey)
    {
        GetPublicStringConstants(typeof(SufiCMSCopilotKeys.Context.ContentAssistant))
            .ShouldContain(expectedKey);
    }

    private static void AssertLocalizedProtocol(
        CopilotSeedTexts texts,
        params string[] requiredTokens)
    {
        foreach (var culture in SupportedCultures)
        {
            texts.DisplayName.ShouldContainKey(culture);
            texts.SystemPrompt.ShouldContainKey(culture);
            texts.SystemPrompt[culture].ShouldNotBeNullOrWhiteSpace();

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

    private static string[] GetPublicStringConstants(Type type)
    {
        return type.GetFields()
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();
    }
}
