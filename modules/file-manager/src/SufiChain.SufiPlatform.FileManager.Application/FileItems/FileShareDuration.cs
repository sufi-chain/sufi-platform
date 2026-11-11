using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

public static class FileShareDuration
{
    public static void Ensure(int durationMinutes)
    {
        var input = new FileShareDurationDto { DurationMinutes = durationMinutes };
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true))
        {
            return;
        }

        throw new AbpValidationException(
            "The share link expiry is outside the allowed range.",
            results);
    }
}
