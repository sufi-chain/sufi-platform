using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Shouldly;
using SufiChain.SufiPlatform.Validation;
using Xunit;

namespace SufiChain.SufiPlatform.Localization;

public class ValidationMessageLocalizationTests
{
    [Fact]
    public void Required_field_message_is_Persian_for_fa()
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            var fa = CultureInfo.GetCultureInfo("fa");
            CultureInfo.CurrentCulture = fa;
            CultureInfo.CurrentUICulture = fa;
            var message = ValidationMessageTemplates.Format(
                new MapLocalizer(("The {0} field is required.", "فیلد {0} الزامی است.")),
                new RequiredAttribute(),
                "نام");
            message.ShouldBe("فیلد نام الزامی است.");
            message.ShouldNotContain("field is required");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    [Fact]
    public void Compare_MinLength_and_Email_use_the_Persian_template_and_a_localized_name()
    {
        var localizer = new MapLocalizer(
            ("'{0}' and '{1}' do not match.", "'{0}' و '{1}' مطابقت ندارند."),
            ("The field {0} must be a string or array type with a minimum length of '{1}'.", "فیلد {0} باید حداقل '{1}' نویسه باشد."),
            ("The {0} field is not a valid e-mail address.", "فیلد {0} یک نشانی ایمیل معتبر نیست."),
            ("Field:NewPassword", "رمز عبور جدید"),
            ("Field:TargetEmailAddress", "نشانی ایمیل گیرنده"));

        var compare = ValidationMessageTemplates.Format(
            localizer,
            new CompareAttribute("NewPassword"),
            ValidationMessageTemplates.LocalizeDisplayName(localizer, "Field:NewPassword"));
        compare.ShouldContain("مطابقت ندارند");
        compare.ShouldContain("رمز عبور جدید");

        var minLength = ValidationMessageTemplates.Format(
            localizer,
            new MinLengthAttribute(6),
            "رمز عبور جدید");
        minLength.ShouldContain("حداقل");
        minLength.ShouldContain("6");

        var email = ValidationMessageTemplates.Format(
            localizer,
            new EmailAddressAttribute(),
            ValidationMessageTemplates.LocalizeDisplayName(localizer, "Field:TargetEmailAddress"));
        email.ShouldContain("نشانی ایمیل گیرنده");
        email.ShouldNotContain("TargetEmailAddress");
    }

    [Fact]
    public void Role_create_submits_through_the_form()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(
                dir.FullName,
                "modules",
                "identity",
                "src",
                "SufiChain.SufiPlatform.Identity.Blazor",
                "Components",
                "RoleCreateModal.razor");
            if (File.Exists(candidate))
            {
                var markup = File.ReadAllText(candidate);
                markup.ShouldContain("id=\"role-create-form\"");
                markup.ShouldContain("form=\"role-create-form\"");
                markup.ShouldContain("Type=\"submit\"");
                markup.ShouldNotContain("OnClick=\"OnValidSubmitAsync\"");
                return;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("RoleCreateModal.razor was not found from the test output directory.");
    }

    [Fact]
    public void Unknown_display_name_stays_the_raw_property_name()
    {
        var localizer = new MapLocalizer(("The {0} field is required.", "فیلد {0} الزامی است."));
        ValidationMessageTemplates.LocalizeDisplayName(localizer, "AdminPassword").ShouldBe("AdminPassword");
    }

    [Fact]
    public void Finance_fa_labels_cash_flow_and_fx_gain_loss()
    {
        var fa = ReadFinanceFa();
        fa.ShouldContain("\"Accounting:CashFlow\": \"جریان نقدی\"");
        fa.ShouldContain("\"Accounting:FxGainLoss\": \"سود و زیان ارز\"");
    }

    private static string ReadFinanceFa()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(
                dir.FullName,
                "pro-modules",
                "finance",
                "src",
                "SufiChain.SufiPlatform.SufiFinance.Domain.Shared",
                "Localization",
                "SufiFinance",
                "fa.json");
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("SufiFinance fa.json was not found from the test output directory.");
    }

    private sealed class MapLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, string> _map;

        public MapLocalizer(params (string Key, string Value)[] entries)
        {
            _map = entries.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        public LocalizedString this[string name]
        {
            get
            {
                var found = _map.TryGetValue(name, out var value);
                return new LocalizedString(name, found ? value! : name, resourceNotFound: !found);
            }
        }

        public LocalizedString this[string name, params object[] arguments] => this[name];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _map.Select(pair => new LocalizedString(pair.Key, pair.Value, false));
    }
}
