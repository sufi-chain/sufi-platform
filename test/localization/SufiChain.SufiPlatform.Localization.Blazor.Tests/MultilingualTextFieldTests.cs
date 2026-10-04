using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using SufiChain.SufiPlatform.Localization.Localization;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Localization.Blazor.Tests;

public class MultilingualTextFieldTests : BunitContext
{
    public MultilingualTextFieldTests()
    {
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>, PassthroughLocalizer>();
        Services.AddSingleton<IStringLocalizer<SufiLocalizationResource>, FaLocalizer>();
    }

    [Fact]
    public void Label_rows_show_every_culture_with_persian_first()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = "خانه",
            ["en"] = "Home",
            ["ar"] = "الرئيسية",
            ["es"] = "Inicio"
        };

        var cut = RenderField(values, fallback: "نام");

        var inputs = cut.FindAll("input");
        inputs.Count.ShouldBe(4);
        inputs[0].GetAttribute("lang").ShouldBe("fa");
        inputs[0].GetAttribute("dir").ShouldBe("rtl");
        inputs[0].GetAttribute("aria-required").ShouldBe("true");
        inputs[1].GetAttribute("lang").ShouldBe("en");
        inputs[1].GetAttribute("dir").ShouldBe("ltr");
        inputs[2].GetAttribute("lang").ShouldBe("ar");
        inputs[2].GetAttribute("dir").ShouldBe("rtl");
        inputs[3].GetAttribute("lang").ShouldBe("es");
        inputs[3].GetAttribute("dir").ShouldBe("ltr");
        cut.Markup.ShouldContain("(اصلی)");
        cut.Markup.ShouldContain("ترجمه شده");
        inputs[0].GetAttribute("value").ShouldBe("خانه");
        inputs[0].GetAttribute("value").ShouldNotBe("Values");
        inputs[0].GetAttribute("value").ShouldNotBe("_displayNames");
        inputs[0].Id.ShouldStartWith("mltf-");
        inputs[0].Id.ShouldEndWith("-fa");

        cut.Render(parameters => parameters
            .Add(parameter => parameter.Values, values)
            .Add(parameter => parameter.Cultures, Cultures())
            .Add(parameter => parameter.FallbackText, "نام جدید"));

        cut.Find("input[lang=en]").GetAttribute("value").ShouldBe("Home");
        cut.Find("input[lang=ar]").GetAttribute("value").ShouldBe("الرئيسية");
        cut.Find("input[lang=es]").GetAttribute("value").ShouldBe("Inicio");
    }

    [Fact]
    public void Two_fields_use_different_row_ids()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["fa"] = "خانه" };
        var first = RenderField(values);
        var second = RenderField(values);

        first.Find("input").Id.ShouldNotBe(second.Find("input").Id);
    }

    [Fact]
    public async Task Empty_persian_label_is_rejected_under_the_fa_row()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = "   ",
            ["en"] = "_localizationKey",
            ["ar"] = "",
            ["es"] = ""
        };

        var cut = RenderField(values);
        var valid = await cut.InvokeAsync(() => cut.Instance.Field!.ValidateAsync());

        valid.ShouldBeFalse();
        var fa = cut.Find("input[lang=fa]");
        fa.GetAttribute("aria-invalid").ShouldBe("true");
        var describedBy = fa.GetAttribute("aria-describedby");
        describedBy.ShouldNotBeNullOrWhiteSpace();
        cut.Find("#" + describedBy).TextContent.ShouldBe("عنوان فارسی الزامی است.");
        cut.Find("input[lang=en]").GetAttribute("aria-invalid").ShouldBe("true");
    }

    [Fact]
    public void Default_row_suggests_the_item_name()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = "",
            ["en"] = "",
            ["ar"] = "",
            ["es"] = ""
        };

        var cut = RenderField(values, fallback: "About", broken: true);

        cut.Find("input[lang=fa]").GetAttribute("placeholder").ShouldBe("About");
        cut.Markup.ShouldContain("خالی بماند: «About» نمایش داده می‌شود");
        cut.Markup.ShouldContain("عنوان این آیتم درست ذخیره نشده بود؛ لطفاً دوباره وارد کنید.");
    }

    [Fact]
    public void Cleared_seeded_language_shows_the_base_text()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = "خانه",
            ["en"] = "Home"
        };
        var bases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "Home"
        };

        var cut = RenderField(values, bases: bases);

        cut.Find("input[lang=en]").GetAttribute("value").ShouldBe("Home");
        cut.Markup.ShouldContain("بازگشت به متن پایه");
    }

    [Fact]
    public void Failed_culture_list_still_shows_a_persian_row()
    {
        var cut = Render<MultilingualTextFieldHost>(parameters => parameters
            .Add(parameter => parameter.Values, new Dictionary<string, string>())
            .Add(parameter => parameter.Cultures, Array.Empty<MultilingualCulture>())
            .Add(parameter => parameter.CulturesFailed, true));

        cut.FindAll("input").Count.ShouldBe(1);
        cut.Find("input").GetAttribute("lang").ShouldBe("fa");
        cut.Find("input").GetAttribute("dir").ShouldBe("rtl");
        cut.Markup.ShouldContain("فهرست زبان‌ها بارگیری نشد؛ فعلاً فقط فارسی.");
        cut.Markup.ShouldNotContain("انتخاب زبان");
    }

    [Fact]
    public void Binding_parameter_receives_the_field_not_its_name()
    {
        var cut = Render<BusinessTextBindingHost>();
        var editor = cut.FindComponent<BusinessTextEditor>();

        editor.Instance.Binding.ShouldNotBeNull();
        editor.Instance.Binding.ShouldBe(cut.Instance.Binding);
        editor.Instance.Binding!.Key.ShouldBe("SeededMenu:public-default:Item:home:DisplayName");
        editor.Instance.Binding.ResourceName.ShouldBe("SufiPlatform");
        editor.Instance.Binding.Key.ShouldNotBe("_binding");
        editor.Instance.Binding.ResourceName.ShouldNotBe("_localizationResourceName");
        editor.Instance.Binding.LiteralValue.ShouldNotBe("_literalDisplayName");
        typeof(BusinessTextEditor).GetProperty(nameof(BusinessTextEditor.Binding))!
            .PropertyType.ShouldBe(typeof(BusinessTextBinding));
    }

    private IRenderedComponent<MultilingualTextFieldHost> RenderField(
        Dictionary<string, string> values,
        string? fallback = null,
        bool broken = false,
        IReadOnlyDictionary<string, string>? bases = null)
    {
        return Render<MultilingualTextFieldHost>(parameters => parameters
            .Add(parameter => parameter.Values, values)
            .Add(parameter => parameter.Cultures, Cultures())
            .Add(parameter => parameter.Required, true)
            .Add(parameter => parameter.FallbackText, fallback)
            .Add(parameter => parameter.ShowBrokenBanner, broken)
            .Add(parameter => parameter.BaseValues, bases));
    }

    private sealed class PassthroughLocalizer : IStringLocalizer<SufiBlazorResource>
    {
        public LocalizedString this[string name] => new(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments] => new(name, name, resourceNotFound: true);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class FaLocalizer : IStringLocalizer<SufiLocalizationResource>
    {
        private static readonly Dictionary<string, string> Map = new()
        {
            ["MultilingualTextField:Primary"] = "(اصلی)",
            ["MultilingualTextField:Untranslated"] = "ترجمه نشده",
            ["MultilingualTextField:Translated"] = "ترجمه شده",
            ["MultilingualTextField:MaxLength"] = "حداکثر {0} نویسه.",
            ["MultilingualTextField:Required"] = "عنوان {0} الزامی است.",
            ["MultilingualTextField:LooksLikeKey"] = "این مقدار شبیه شناسهٔ سیستمی است؛ متن قابل نمایش وارد کنید.",
            ["MultilingualTextField:Broken"] = "عنوان این آیتم درست ذخیره نشده بود؛ لطفاً دوباره وارد کنید.",
            ["MultilingualTextField:CulturesFailed"] = "فهرست زبان‌ها بارگیری نشد؛ فعلاً فقط فارسی.",
            ["MultilingualTextField:EmptyFallback"] = "خالی بماند: «{0}» نمایش داده می‌شود",
            ["MultilingualTextField:ScriptWarning"] = "به نظر می‌رسد این متن به زبان دیگری است.",
            ["MultilingualTextField:BaseText"] = "بازگشت به متن پایه"
        };

        public LocalizedString this[string name] => Format(name);

        public LocalizedString this[string name, params object[] arguments] => Format(name, arguments);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            Map.Select(pair => new LocalizedString(pair.Key, pair.Value));

        private static LocalizedString Format(string name, params object[] arguments)
        {
            var template = Map.TryGetValue(name, out var value) ? value : name;
            var text = arguments.Length == 0 ? template : string.Format(template, arguments);
            return new LocalizedString(name, text, resourceNotFound: !Map.ContainsKey(name));
        }
    }

    private static List<MultilingualCulture> Cultures() =>
    [
        new() { CultureName = "fa", DisplayName = "فارسی", IsRtl = true, IsDefault = true },
        new() { CultureName = "en", DisplayName = "English", IsRtl = false },
        new() { CultureName = "ar", DisplayName = "العربية", IsRtl = true },
        new() { CultureName = "es", DisplayName = "Español", IsRtl = false }
    ];
}
