using Volo.Abp.Validation.StringValues;

namespace SufiChain.SufiPlatform.Features;

/// <summary>
/// Free-text string value type, optionally constrained by a value validator.
/// </summary>
public class FreeTextStringValueType : Volo.Abp.Validation.StringValues.FreeTextStringValueType,
    IStringValueType
{
    public FreeTextStringValueType()
    {
    }

    public FreeTextStringValueType(IValueValidator validator)
        : base(validator)
    {
    }
}
