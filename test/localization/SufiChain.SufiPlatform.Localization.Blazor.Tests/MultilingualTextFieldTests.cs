using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Localization.Blazor.Tests;

public class MultilingualTextFieldTests : BunitContext
{
    public MultilingualTextFieldTests()
    {
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>, PassthroughLocalizer>();
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

        var cut = Render<MultilingualTextField>(parameters => parameters
            .Add(parameter => parameter.Values, values)
            .Add(parameter => parameter.Cultures, Cultures())
            .Add(parameter => parameter.Required, true)
            .Add(parameter => parameter.FallbackText, "نام"));

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
        inputs[0].GetAttribute("value").ShouldBe("خانه");
        inputs[0].GetAttribute("value").ShouldNotBe("Values");
        inputs[0].GetAttribute("value").ShouldNotBe("_displayNames");

        cut.Render(parameters => parameters
            .Add(parameter => parameter.Values, values)
            .Add(parameter => parameter.Cultures, Cultures())
            .Add(parameter => parameter.FallbackText, "نام جدید"));

        cut.Find("input[lang=en]").GetAttribute("value").ShouldBe("Home");
        cut.Find("input[lang=ar]").GetAttribute("value").ShouldBe("الرئيسية");
        cut.Find("input[lang=es]").GetAttribute("value").ShouldBe("Inicio");
    }

    [Fact]
    public async Task Empty_persian_label_is_rejected_under_the_fa_row()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = "   ",
            ["en"] = "",
            ["ar"] = "",
            ["es"] = ""
        };

        var cut = Render<MultilingualTextField>(parameters => parameters
            .Add(parameter => parameter.Values, values)
            .Add(parameter => parameter.Cultures, Cultures())
            .Add(parameter => parameter.Required, true));

        var valid = await cut.InvokeAsync(() => cut.Instance.ValidateAsync());

        valid.ShouldBeFalse();
        cut.Find("#mltf-fa-error").TextContent.ShouldBe("عنوان فارسی الزامی است.");
    }

    [Fact]
    public void Failed_culture_list_still_shows_a_persian_row()
    {
        var cut = Render<MultilingualTextField>(parameters => parameters
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

    private sealed class PassthroughLocalizer : IStringLocalizer<SufiBlazorResource>
    {
        public LocalizedString this[string name] => new(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments] => new(name, name, resourceNotFound: true);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private static List<MultilingualCulture> Cultures() =>
    [
        new() { CultureName = "fa", DisplayName = "فارسی", IsRtl = true, IsDefault = true },
        new() { CultureName = "en", DisplayName = "English", IsRtl = false },
        new() { CultureName = "ar", DisplayName = "العربية", IsRtl = true },
        new() { CultureName = "es", DisplayName = "Español", IsRtl = false }
    ];
}
