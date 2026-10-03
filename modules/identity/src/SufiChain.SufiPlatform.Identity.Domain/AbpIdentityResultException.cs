using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Identity.Localization;
using Volo.Abp;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Identity;

public class AbpIdentityResultException : BusinessException, ILocalizeErrorMessage
{
    public IdentityResult IdentityResult { get; }

    public AbpIdentityResultException([NotNull] IdentityResult identityResult)
        : base(message: identityResult.Errors.Select(err => err.Description).JoinAsString(", "))
    {
        IdentityResult = Check.NotNull(identityResult, nameof(identityResult));
    }

    public string LocalizeMessage(LocalizationContext context)
    {
        var localizer = context.ServiceProvider.GetRequiredService<IStringLocalizer<SufiIdentityResource>>();
        return IdentityResult.Errors
            .Select(error => LocalizeError(localizer, error))
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .JoinAsString(", ");
    }

    private static string LocalizeError(IStringLocalizer localizer, IdentityError error)
    {
        if (string.IsNullOrWhiteSpace(error.Code))
        {
            return error.Description;
        }

        var localized = localizer[$"IdentityError:{error.Code}"];
        return localized.ResourceNotFound ? error.Description : localized.Value;
    }
}
