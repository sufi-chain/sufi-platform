using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Localization.Dtos;

public class CreateUpdateLocalizationTextDto
{
    [Required]
    [StringLength(128)]
    public string ResourceName { get; set; } = default!;

    [Required]
    [StringLength(16)]
    public string CultureName { get; set; } = default!;

    [Required]
    [StringLength(512)]
    public string Key { get; set; } = default!;

    [Required]
    [StringLength(4096)]
    public string Value { get; set; } = default!;
}

public class UpdateLocalizationTextValueDto
{
    [Required]
    [StringLength(4096)]
    public string Value { get; set; } = default!;
}
