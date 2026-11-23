using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.Settings;

/// <summary>
/// Server-side checks for email settings. An empty from-address is allowed.
/// A malformed one is an ABP validation error that names <see cref="UpdateEmailSettingsDto.DefaultFromAddress"/>.
/// </summary>
public static class EmailSettingsRules
{
    public const int DefaultSmtpPort = 587;

    public static void RejectIncompleteSmtp(string? host, int port, string hostMessage, string portMessage)
    {
        var results = new List<ValidationResult>();
        if (string.IsNullOrWhiteSpace(host))
        {
            results.Add(new ValidationResult(hostMessage, [nameof(UpdateEmailSettingsDto.SmtpHost)]));
        }

        if (port < 1 || port > 65535)
        {
            results.Add(new ValidationResult(portMessage, [nameof(UpdateEmailSettingsDto.SmtpPort)]));
        }

        if (results.Count > 0)
        {
            throw new AbpValidationException(results);
        }
    }

    public static void RejectInvalidFromAddress(string? address, string message)
    {
        if (string.IsNullOrWhiteSpace(address) || IsEmailAddress(address))
        {
            return;
        }

        throw new AbpValidationException(
        [
            new ValidationResult(message, [nameof(UpdateEmailSettingsDto.DefaultFromAddress)])
        ]);
    }

    public static bool IsEmailAddress(string address)
    {
        if (!new EmailAddressAttribute().IsValid(address))
        {
            return false;
        }

        var at = address.LastIndexOf('@');
        return at > 0 && address[(at + 1)..].Contains('.');
    }
}
