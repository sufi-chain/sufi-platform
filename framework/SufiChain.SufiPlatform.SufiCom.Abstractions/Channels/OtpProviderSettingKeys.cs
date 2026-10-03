namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Provider setting keys shared by OTP-capable channels. Template values are provider-specific
/// (a Kavenegar template name or an SMS.ir template id).
/// </summary>
public static class OtpProviderSettingKeys
{
    public const string Template = "OtpTemplate";

    public const string TemplateLogin = "OtpTemplateLogin";

    public const string TemplateRegistration = "OtpTemplateRegistration";

    public const string TemplateTwoFactor = "OtpTemplateTwoFactor";

    public const string TemplatePhoneConfirmation = "OtpTemplatePhoneConfirmation";

    public const string CodeParameterName = "OtpCodeParameterName";

    public const string SenderNumber = "OtpSenderNumber";

    public static readonly string[] PurposeTemplateKeys =
    {
        TemplateLogin,
        TemplateRegistration,
        TemplateTwoFactor,
        TemplatePhoneConfirmation
    };

    /// <summary>
    /// Localization key returned by <see cref="ISmsOtpChannel.GetOtpConfigurationError"/> when no OTP template is set.
    /// </summary>
    public const string TemplateRequiredError = "Providers.Otp.TemplateRequired";

    /// <summary>
    /// Builds the default and per-purpose OTP template fields. Fields are optional for the generic
    /// completeness check so plain SMS keeps working; the OTP route reports a missing default template instead.
    /// </summary>
    public static List<ProviderSettingField> CreateTemplateFields(string providerKeyPrefix, string? validationPattern)
    {
        var fields = new List<ProviderSettingField>
        {
            new()
            {
                Key = Template,
                Label = "Providers.Otp.Template.Label",
                FieldType = "text",
                IsRequired = false,
                Placeholder = providerKeyPrefix + ".OtpTemplate.Placeholder",
                HelpText = providerKeyPrefix + ".OtpTemplate.HelpText",
                ValidationPattern = validationPattern
            }
        };

        foreach (var key in PurposeTemplateKeys)
        {
            fields.Add(new ProviderSettingField
            {
                Key = key,
                Label = "Providers.Otp." + key + ".Label",
                FieldType = "text",
                IsRequired = false,
                Placeholder = providerKeyPrefix + ".OtpTemplate.Placeholder",
                HelpText = "Providers.Otp.PurposeTemplate.HelpText",
                ValidationPattern = validationPattern
            });
        }

        return fields;
    }

    /// <summary>
    /// Returns the purpose-specific template when configured; otherwise the default <see cref="Template"/>.
    /// </summary>
    public static string? ResolveTemplate(IReadOnlyDictionary<string, string> settings, string purpose)
    {
        var purposeKey = purpose switch
        {
            OtpPurposes.Login => TemplateLogin,
            OtpPurposes.Registration => TemplateRegistration,
            OtpPurposes.TwoFactor => TemplateTwoFactor,
            OtpPurposes.PhoneConfirmation => TemplatePhoneConfirmation,
            _ => null
        };

        if (purposeKey != null &&
            settings.TryGetValue(purposeKey, out var purposeTemplate) &&
            !string.IsNullOrWhiteSpace(purposeTemplate))
        {
            return purposeTemplate.Trim();
        }

        return settings.TryGetValue(Template, out var template) && !string.IsNullOrWhiteSpace(template)
            ? template.Trim()
            : null;
    }
}
