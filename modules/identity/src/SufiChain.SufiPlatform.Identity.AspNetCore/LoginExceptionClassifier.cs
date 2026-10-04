using System.Data.Common;
using Volo.Abp;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Turns a sign-in exception into a localization key and a log reason.
/// The reason is safe to log: exception type and error code, not passwords, tokens, or connection strings.
/// </summary>
public static class LoginExceptionClassifier
{
    public static string LocalizationKey(Exception exception, bool tenantSelected)
    {
        if (exception == null)
        {
            return "LoginUnexpectedError";
        }

        if (IsConcurrencyFailure(exception))
        {
            return "LoginConcurrencyError";
        }

        if (TryGetBusinessException(exception, out var businessException) &&
            !string.IsNullOrWhiteSpace(businessException.Code))
        {
            if (IsTenantResolveCode(businessException.Code))
            {
                return tenantSelected ? "LoginTenantMismatch" : "LoginAccountStoreUnavailable";
            }

            return businessException.Code;
        }

        if (exception is AbpIdentityResultException identityException)
        {
            var code = identityException.IdentityResult.Errors.FirstOrDefault()?.Code;
            return string.IsNullOrWhiteSpace(code)
                ? "LoginUnexpectedError"
                : "IdentityError:" + code;
        }

        if (IsDataAccessFailure(exception))
        {
            return "LoginAccountStoreUnavailable";
        }

        return "LoginUnexpectedError";
    }

    public static string ReasonCode(Exception exception)
    {
        if (exception == null)
        {
            return "none";
        }

        if (TryGetBusinessException(exception, out var businessException) &&
            !string.IsNullOrWhiteSpace(businessException.Code))
        {
            return businessException.GetType().Name + ":" + businessException.Code;
        }

        var innermost = exception;
        while (innermost.InnerException != null)
        {
            innermost = innermost.InnerException;
        }

        return innermost.GetType().FullName ?? innermost.GetType().Name;
    }

    public static bool IsDataAccessFailure(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is DbException or TimeoutException)
            {
                return true;
            }

            var name = current.GetType().Name;
            if (name.Contains("SqlException", StringComparison.Ordinal) ||
                name.Contains("Mongo", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsConcurrencyFailure(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is AbpDbConcurrencyException)
            {
                return true;
            }

            if (string.Equals(current.GetType().Name, "DbUpdateConcurrencyException", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetBusinessException(Exception exception, out BusinessException businessException)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is BusinessException match)
            {
                businessException = match;
                return true;
            }
        }

        businessException = null!;
        return false;
    }

    private static bool IsTenantResolveCode(string code)
    {
        return code.Contains("010001", StringComparison.Ordinal) ||
               code.Contains("010002", StringComparison.Ordinal) ||
               code.Contains("TenantNotFound", StringComparison.OrdinalIgnoreCase) ||
               code.Contains("TenantNotActive", StringComparison.OrdinalIgnoreCase);
    }
}
