using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Localization.Dtos;

public class CreateUpdateLocalizationTextDto
{
    [Required]
    [StringLength(LocalizationTextConsts.MaxResourceNameLength)]
    public string ResourceName { get; set; } = default!;

    [Required]
    [StringLength(LocalizationTextConsts.MaxCultureNameLength)]
    public string CultureName { get; set; } = default!;

    [Required]
    [StringLength(LocalizationTextConsts.MaxKeyLength)]
    public string Key { get; set; } = default!;

    [Required]
    [StringLength(LocalizationTextConsts.MaxValueLength)]
    public string Value { get; set; } = default!;
}

public class UpdateLocalizationTextValueDto
{
    [Required]
    [StringLength(LocalizationTextConsts.MaxValueLength)]
    public string Value { get; set; } = default!;
}
