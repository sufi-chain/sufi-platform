using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.Validation;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IAttributeValidationResultProvider))]
public class LocalizedAttributeValidationResultProvider : DefaultAttributeValidationResultProvider, ITransientDependency
{
    private readonly IStringLocalizer<SufiValidationResource> _localizer;

    public LocalizedAttributeValidationResultProvider(IStringLocalizer<SufiValidationResource> localizer)
    {
        _localizer = localizer;
    }

    public override ValidationResult? GetOrDefault(
        ValidationAttribute validationAttribute,
        object? validatingObject,
        ValidationContext validationContext)
    {
        if (validationAttribute.IsValid(validatingObject))
        {
            return ValidationResult.Success;
        }

        var message = ValidationMessageTemplates.Format(_localizer, validationAttribute, validationContext.DisplayName);
        return new ValidationResult(message, validationContext.MemberName == null ? null : new[] { validationContext.MemberName });
    }
}

public static class ValidationMessageTemplates
{
    public const string Required = "The {0} field is required.";

    public static string Format(IStringLocalizer localizer, ValidationAttribute attribute, string displayName)
    {
        if (!string.IsNullOrEmpty(attribute.ErrorMessage))
        {
            return attribute.FormatErrorMessage(displayName);
        }

        if (attribute is StringLengthAttribute length)
        {
            return Format(localizer, "The field {0} must be a string with a maximum length of {1}.", displayName, length.MaximumLength);
        }

        if (attribute is MaxLengthAttribute maxLength)
        {
            return Format(localizer, "The field {0} must be a string with a maximum length of {1}.", displayName, maxLength.Length);
        }

        if (attribute is MinLengthAttribute minLength)
        {
            return Format(localizer, "The field {0} must be a string or array type with a minimum length of '{1}'.", displayName, minLength.Length);
        }

        if (attribute is RangeAttribute range)
        {
            return Format(localizer, "The field {0} must be between {1} and {2}.", displayName, range.Minimum, range.Maximum);
        }

        var key = attribute switch
        {
            RequiredAttribute => Required,
            EmailAddressAttribute => "The {0} field is not a valid e-mail address.",
            PhoneAttribute => "The {0} field is not a valid phone number.",
            UrlAttribute => "The {0} field is not a valid fully-qualified http, https, or ftp URL.",
            CreditCardAttribute => "The {0} field is not a valid credit card number.",
            _ => null
        };

        if (key == null)
        {
            return attribute.FormatErrorMessage(displayName);
        }

        var localized = localizer[key];
        var template = localized.ResourceNotFound ? key : localized.Value;
        return string.Format(CultureInfo.CurrentCulture, template, displayName);
    }

    private static string Format(IStringLocalizer localizer, string key, params object?[] args)
    {
        var localized = localizer[key];
        var template = localized.ResourceNotFound ? key : localized.Value;
        return string.Format(CultureInfo.CurrentCulture, template, args);
    }
}
