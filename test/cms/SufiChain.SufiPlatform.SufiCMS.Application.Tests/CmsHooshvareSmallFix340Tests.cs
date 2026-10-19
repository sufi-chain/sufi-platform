using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiCMS.Blocks;
using SufiChain.SufiPlatform.SufiCMS.Data;
using SufiChain.SufiPlatform.SufiCMS.Hooshvare.Mcp;
using SufiChain.SufiPlatform.SufiCMS.Localization;
using SufiChain.SufiPlatform.SufiCMS.PendingActions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

/// <summary>
/// sufi-chain#340 small fix (9.0.1): proposals found again after a reload, actionable CMS tool errors, form keys for the
/// form block, and draft text in the page language.
/// </summary>
public class CmsHooshvareSmallFix340Tests
{
    private static readonly string[] Cultures = ["en", "fa", "ar", "es"];
    private static readonly Regex PlaceholderPattern = new(@"\{[A-Za-z]+\}");
    private static readonly Regex ToolNamePattern = new(@"cms(?:\.[a-z_]+)+");

    [Fact]
    public async Task Pending_lookup_passes_the_page_and_leaves_out_expired_proposals()
    {
        var userId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var now = new DateTime(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);
        var open = new CmsPendingAction(Guid.NewGuid(), CmsPendingActionKind.Redirect, "open", "{}", userId, null, now.AddMinutes(5), pageId);
        var expired = new CmsPendingAction(Guid.NewGuid(), CmsPendingActionKind.Redirect, "expired", "{}", userId, null, now.AddMinutes(-1), pageId);
        var repository = Substitute.For<ICmsPendingActionRepository>();
        repository.GetListByUserAsync(userId, null, CmsPendingActionStatus.Pending, pageId, Arg.Any<CancellationToken>())
            .Returns(new List<CmsPendingAction> { open, expired });

        var result = await PendingActions(repository, userId, now)
            .GetListAsync(new GetCmsPendingActionsInput { PageId = pageId, Status = CmsPendingActionStatus.Pending });

        result.Select(action => action.Id).ShouldBe(new[] { open.Id });
        result[0].PageId.ShouldBe(pageId);
    }

    [Fact]
    public async Task Session_lookup_without_a_status_keeps_every_row()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var now = new DateTime(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);
        var old = new CmsPendingAction(Guid.NewGuid(), CmsPendingActionKind.Upload, "old", "{}", userId, sessionId, now.AddMinutes(-1));
        var repository = Substitute.For<ICmsPendingActionRepository>();
        repository.GetListByUserAsync(userId, sessionId, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<CmsPendingAction> { old });

        var result = await PendingActions(repository, userId, now)
            .GetListAsync(new GetCmsPendingActionsInput { HooshvareSessionId = sessionId });

        result.Select(action => action.Id).ShouldBe(new[] { old.Id });
    }

    [Fact]
    public void Style_guard_error_names_the_rejected_styles_and_the_fix()
    {
        var text = new CmsMcpToolErrorDescriber(Localizer("en")).Describe(
            CMSErrorCodes.StyleGuardRejected,
            new Dictionary<string, object?> { ["issues"] = "InlineStyle: style=\"color:red\"" });

        text.ShouldNotBeNull();
        text.ShouldContain("InlineStyle: style=\"color:red\"");
        text.ShouldContain("cms.get_site_context");
        text.ShouldContain("cms.propose_style_change");
        text.ShouldNotContain("{issues}");
    }

    [Fact]
    public void Save_on_a_page_with_content_tells_the_model_to_patch_or_insert_a_section()
    {
        var text = new CmsMcpToolErrorDescriber(Localizer("en")).Describe(CMSErrorCodes.PageDraftNotEmpty, new Dictionary<string, object?>());

        text.ShouldNotBeNull();
        text.ShouldContain("cms.patch_page_section");
        text.ShouldContain("cms.insert_section");
        text.ShouldContain("cms.get_page");
    }

    [Fact]
    public void Placeholders_take_the_exception_data_or_are_dropped()
    {
        var describer = new CmsMcpToolErrorDescriber(Localizer("fa"));

        describer.Describe(CMSErrorCodes.SectionNotFound, new Dictionary<string, object?> { ["sectionId"] = "s-hero" })
            .ShouldNotBeNull().ShouldContain("s-hero");
        describer.Describe(CMSErrorCodes.SectionNotFound, new Dictionary<string, object?>())
            .ShouldNotBeNull().ShouldNotContain("{");
        describer.Describe(CMSErrorCodes.PageLimitReached, new Dictionary<string, object?> { ["limit"] = 25 })
            .ShouldNotBeNull().ShouldContain("25");
    }

    [Fact]
    public void Codes_without_a_tool_text_are_left_to_the_bare_code()
    {
        var describer = new CmsMcpToolErrorDescriber(Localizer("en"));

        describer.Describe("CMS:NoSuchCode", new Dictionary<string, object?>()).ShouldBeNull();
        describer.Describe("Forms:FormNotFound", new Dictionary<string, object?>()).ShouldBeNull();
        describer.Describe(string.Empty, new Dictionary<string, object?>()).ShouldBeNull();
    }

    [Fact]
    public void Tool_error_texts_exist_in_every_culture_with_the_same_placeholders_and_real_tool_names()
    {
        var tools = typeof(SufiCMSHooshvareKeys.Tools)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToHashSet(StringComparer.Ordinal);
        var english = Texts("en")
            .Where(text => text.Key.StartsWith(CmsMcpToolErrorDescriber.KeyPrefix, StringComparison.Ordinal))
            .ToDictionary(text => text.Key, text => text.Value);
        english.Keys.ShouldContain("CmsTool:StyleGuardRejected");
        english.Keys.ShouldContain("CmsTool:PageDraftNotEmpty");
        english.Count.ShouldBeGreaterThanOrEqualTo(12);

        foreach (var culture in Cultures)
        {
            var texts = Texts(culture);
            texts.ShouldContainKey("CmsHooshvare:PendingRestored");
            foreach (var (key, englishText) in english)
            {
                texts.ShouldContainKey(key, culture + " " + key);
                Matches(PlaceholderPattern, texts[key]).ShouldBe(Matches(PlaceholderPattern, englishText), ignoreOrder: true, culture + " " + key);
                Matches(ToolNamePattern, texts[key]).ShouldBe(Matches(ToolNamePattern, englishText), ignoreOrder: true, culture + " " + key);
                foreach (var tool in Matches(ToolNamePattern, texts[key]))
                {
                    tools.ShouldContain(tool, culture + " " + key);
                }
            }
        }
    }

    [Fact]
    public void Persian_texts_use_the_agreed_terms()
    {
        var fa = Texts("fa");
        fa["CmsHooshvare:PendingRestored"].ShouldContain("گفت\u200Cوگو");
        foreach (var (key, text) in fa.Where(text => text.Key.StartsWith(CmsMcpToolErrorDescriber.KeyPrefix, StringComparison.Ordinal)
                                                     || text.Key == "CmsHooshvare:PendingRestored"))
        {
            text.ShouldNotContain("پرامپت", Case.Sensitive, key);
            text.ShouldNotContain("گفتگو", Case.Sensitive, key);
        }
    }

    [Fact]
    public async Task List_blocks_offers_form_keys_and_an_empty_list_when_a_source_fails()
    {
        var registry = Substitute.For<ICmsBlockRegistry>();
        registry.GetAll().Returns(new List<CmsBlockDescriptor>
        {
            new()
            {
                Name = "form",
                DescriptionKey = "Block:Form",
                Parameters = { new CmsBlockParameterDescriptor { Name = "form", Kind = CmsBlockParameterKind.FormId, Required = true } }
            },
            new()
            {
                Name = "menu",
                DescriptionKey = "Block:Menu",
                Parameters =
                {
                    new CmsBlockParameterDescriptor { Name = "menu", Kind = CmsBlockParameterKind.MenuId },
                    new CmsBlockParameterDescriptor { Name = "title", Kind = CmsBlockParameterKind.Text }
                }
            }
        });
        var forms = Substitute.For<ICmsBlockParameterValueSource>();
        forms.Kind.Returns(CmsBlockParameterKind.FormId);
        forms.GetValuesAsync().Returns(Task.FromResult<IReadOnlyList<CmsBlockParameterValue>>(
            new List<CmsBlockParameterValue> { new() { Value = "contact", Label = "Contact us" } }));
        var menus = Substitute.For<ICmsBlockParameterValueSource>();
        menus.Kind.Returns(CmsBlockParameterKind.MenuId);
        menus.GetValuesAsync().Returns(Task.FromException<IReadOnlyList<CmsBlockParameterValue>>(new InvalidOperationException("no permission")));
        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(call => new LocalizedString(call.Arg<string>(), call.Arg<string>()));
        var localizerFactory = Substitute.For<IStringLocalizerFactory>();
        localizerFactory.Create(Arg.Any<Type>()).Returns(localizer);
        var services = new ServiceCollection()
            .AddSingleton(registry)
            .AddSingleton(forms)
            .AddSingleton(menus)
            .AddSingleton(localizerFactory)
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var service = new CmsReadMcpAppService { LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()) };

        var blocks = JsonSerializer.SerializeToElement(await service.ListBlocksAsync());

        var form = Block(blocks, "form").GetProperty("Parameters")[0].GetProperty("AllowedValues");
        form.GetArrayLength().ShouldBe(1);
        form[0].GetProperty("Value").GetString().ShouldBe("contact");
        form[0].GetProperty("Label").GetString().ShouldBe("Contact us");
        var menu = Block(blocks, "menu").GetProperty("Parameters");
        menu[0].GetProperty("AllowedValues").GetArrayLength().ShouldBe(0);
        menu[1].GetProperty("AllowedValues").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public void Draft_prompts_keep_text_in_the_page_language_and_script()
    {
        foreach (var texts in new[]
                 {
                     SufiCMSHooshvareSeedTexts.SiteBuilder.Texts,
                     SufiCMSHooshvareSeedTexts.Writer.Texts,
                     SufiCMSHooshvareSeedTexts.SiteOps.Texts
                 })
        {
            foreach (var culture in Cultures)
            {
                var prompt = texts.SystemPrompt[culture];
                prompt.ShouldContain("only in the page culture's language and script");
                prompt.ShouldContain("Never mix in words or letters of another language or script");
                prompt.Length.ShouldBeLessThanOrEqualTo(LocalizationTextConsts.MaxValueLength);
            }
        }
    }

    private static CmsPendingActionAppService PendingActions(ICmsPendingActionRepository repository, Guid userId, DateTime now)
    {
        var user = Substitute.For<ICurrentUser>();
        user.Id.Returns(userId);
        user.IsAuthenticated.Returns(true);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(now);
        var services = new ServiceCollection().AddSingleton(user).AddSingleton(clock);
        return new CmsPendingActionAppService(repository, null!)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider())
        };
    }

    private static JsonElement Block(JsonElement blocks, string name) =>
        blocks.EnumerateArray().Single(block => block.GetProperty("Name").GetString() == name);

    private static string[] Matches(Regex pattern, string text) =>
        pattern.Matches(text).Select(match => match.Value).Distinct(StringComparer.Ordinal).ToArray();

    private static IStringLocalizer<SufiCMSResource> Localizer(string culture)
    {
        var texts = Texts(culture);
        var localizer = Substitute.For<IStringLocalizer<SufiCMSResource>>();
        localizer[Arg.Any<string>()].Returns(call =>
        {
            var name = call.Arg<string>();
            return texts.TryGetValue(name, out var value)
                ? new LocalizedString(name, value)
                : new LocalizedString(name, name, resourceNotFound: true);
        });
        return localizer;
    }

    private static Dictionary<string, string> Texts(string culture)
    {
        var assembly = typeof(SufiCMSResource).Assembly;
        var resource = assembly.GetManifestResourceNames()
            .Single(name => name.EndsWith(".Localization.Cms." + culture + ".json", StringComparison.OrdinalIgnoreCase));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var document = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });
        return document.RootElement.GetProperty("texts").EnumerateObject()
            .ToDictionary(text => text.Name, text => text.Value.GetString() ?? string.Empty, StringComparer.Ordinal);
    }
}
